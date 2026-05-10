using System.Collections;
using UnityEngine;
using SpaceRunner.World;  // pre DistanceTracker

namespace SpaceRunner.Meteorites
{
    public class MeteoriteSpawner : MonoBehaviour
    {
        public enum MeteoriteSize { Small, Medium, Large }

        // Skupina parametrov pre jednu veľkosť. Nested class so [System.Serializable]
        // je Unity konvencia pre štruktúrované grouping v Inspectore — zobrazí sa
        // ako foldable group, parametre logicky pri sebe.
        [System.Serializable]
        public class SizeData
        {
            public float minSpeed = 1f;
            public float maxSpeed = 3f;
            public float minRotationDegPerSec = 30f;
            public float maxRotationDegPerSec = 90f;
            public GameObject[] prefabs;  // varianty pre túto veľkosť
        }

        [Header("Dependencies")]
        [SerializeField] private DistanceTracker _distanceTracker;
        [SerializeField] private Transform _meteoritesParent;  // typicky WallConveyor — meteority dedia world scroll
        [SerializeField] private float _levelTargetDistance = 1000f;

        [Header("Spawn rate (meteoritov/sekundu)")]
        [SerializeField] private float _spawnRateStart = 0.5f;
        [SerializeField] private float _spawnRateEnd = 4f;

        [Header("Spawn position")]
        [SerializeField] private float _spawnLineY = 6f;
        [SerializeField] private float _spawnXMin = -3f;
        [SerializeField] private float _spawnXMax = 3f;

        [Header("Spawn cone")]
        [SerializeField] private float _coneHalfAngleDegrees = 30f;

        [Header("Size distribution (relative weights)")]
        [SerializeField] private float _weightSmall = 1f;
        [SerializeField] private float _weightMedium = 1f;
        [SerializeField] private float _weightLarge = 1f;

        [Header("Per-size data")]
        [SerializeField] private SizeData _smallData;
        [SerializeField] private SizeData _mediumData;
        [SerializeField] private SizeData _largeData;

        private Coroutine _spawnRoutine;

        private void OnEnable()
        {
            _spawnRoutine = StartCoroutine(SpawnLoop());
        }

        private void OnDisable()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        private IEnumerator SpawnLoop()
        {
            // coroutine spawn loop
            while (true)
            {
                // Vypočítaj progress v leveli
                float progress = Mathf.Clamp01(_distanceTracker.CurrentDistance / _levelTargetDistance);

                // Vypočítaj currentRate
                float currentRate = Mathf.Lerp(_spawnRateStart, _spawnRateEnd, progress);

                // vypočítaj interval
                float interval = 1f / currentRate;
                
                // čakaj kým máš spawnúť meteorit
                yield return new WaitForSeconds(interval);

                // spawn meteoritu
                SpawnMeteorite();

            }
            
        }

        private void SpawnMeteorite()
        {
            // 1. Vyber veľkosť podľa váh
            MeteoriteSize size = PickSize();
            SizeData data = GetSizeData(size);

            // 2. Vyber náhodný sprite variant
            GameObject prefab = data.prefabs[Random.Range(0, data.prefabs.Length)];

            // 3. Vyber spawn pozíciu (random x, fixné y)
            float spawnX = Random.Range(_spawnXMin, _spawnXMax);
            Vector2 spawnPos = new Vector2(spawnX, _spawnLineY);

            // 4. Náhodný počiatočný uhol natočenia sprite-u (uniformne 0-360°)
            float startAngle = Random.Range(0f, 360f);

            // Vypočítaj velocity vektor
            // delta v stupňoch: random z [-_coneHalfAngleDegrees, +_coneHalfAngleDegrees]
            float randomDelta = Random.Range(-_coneHalfAngleDegrees, _coneHalfAngleDegrees);

            // konverzia na radiány
            float randomDeltaRad = Mathf.Deg2Rad * randomDelta;

            // smer (jednotkový vektor): (sin δ, -cos δ) — vzorec pre kužeľ od vertikály dole
            Vector2 direction = new Vector2(Mathf.Sin(randomDeltaRad), -Mathf.Cos(randomDeltaRad));

            // speed: random z [data.minSpeed, data.maxSpeed]
            float speed = Random.Range(data.minSpeed, data.maxSpeed);

            // velocity: smer × speed
            Vector2 velocity = direction * speed;  

            // 6. Vypočítaj signed rotation rate
            float rotMagnitude = Random.Range(data.minRotationDegPerSec, data.maxRotationDegPerSec);
            float rotSign = Random.value < 0.5f ? -1f : 1f;
            float signedRotation = rotMagnitude * rotSign;

            // 7. Instantiate + Initialize
            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0f, 0f, startAngle), _meteoritesParent);
            Meteorite meteorite = obj.GetComponent<Meteorite>();
            meteorite.Initialize(velocity, signedRotation);
        }

        private MeteoriteSize PickSize()
        {
            float total = _weightSmall + _weightMedium + _weightLarge;
            float roll = Random.Range(0f, total);
            if (roll < _weightSmall) return MeteoriteSize.Small;
            if (roll < _weightSmall + _weightMedium) return MeteoriteSize.Medium;
            return MeteoriteSize.Large;
        }

        private SizeData GetSizeData(MeteoriteSize size)
        {
            switch (size)
            {
                case MeteoriteSize.Small: return _smallData;
                case MeteoriteSize.Medium: return _mediumData;
                case MeteoriteSize.Large: return _largeData;
                default: return _smallData;
            }
        }
    }
}