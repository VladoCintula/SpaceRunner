using System;
using System.Collections;
using UnityEngine;
using SpaceRunner.World;
using Random = UnityEngine.Random; 

namespace SpaceRunner.Meteorites
{
    /// <summary>
    /// Spawns meteorites at a level-progress-driven rate and configures each one
    /// from a per-size data block (speed, rotation, mass, prefab variants).
    ///
    /// Spawn loop runs in a coroutine; spawn rate is linearly interpolated between
    /// _spawnRateStart and _spawnRateEnd based on _distanceTracker.CurrentDistance
    /// vs _levelTargetDistance. Each meteorite is parented to _meteoritesParent
    /// (currently the WallConveyor) so it inherits the world scroll and stays in
    /// the same reference frame as the corridor walls.
    ///
    /// Velocity direction is drawn from a downward cone of ±_coneHalfAngleDegrees
    /// from vertical, magnitude from the per-size range. Mass comes from SizeData
    /// and is the single source for the elastic collision formula in Meteorite.cs.
    ///
    /// Design rationale: 21.01.02 Meteority.md, sections "Pohyb meteoritov" and "Generovanie meteoritov".
    /// </summary>
    public class MeteoriteSpawner : MonoBehaviour
    {
        /// <summary>Categorical meteorite size; selects which SizeData block applies and which prefab pool to draw from.</summary>
        public enum MeteoriteSize { Small, Medium, Large }

        /// <summary>
        /// Per-size parameter block. Grouped as a nested [System.Serializable] class
        /// so the Inspector renders it as a foldable group of related fields.
        /// </summary>
        [System.Serializable]
        public class SizeData
        {
            [Tooltip("Lower bound of the speed range for this size (world units / sec).")]
            public float minSpeed = 1f;

            [Tooltip("Upper bound of the speed range for this size (world units / sec).")]
            public float maxSpeed = 3f;

            [Tooltip("Lower bound of the visual rotation rate for this size (deg / sec). Sign is randomized at spawn.")]
            public float minRotationDegPerSec = 30f;

            [Tooltip("Upper bound of the visual rotation rate for this size (deg / sec).")]
            public float maxRotationDegPerSec = 90f;

            [Tooltip("Mass used in the elastic collision formula in Meteorite. Single source for the meteorite — Rigidbody2D.mass is ignored for Kinematic bodies.")]
            public float mass = 1f;

            [Tooltip("Prefab variants for this size — one is picked uniformly at random per spawn.")]
            public GameObject[] prefabs;
        }

        [Header("Dependencies")]
        [Tooltip("Source of CurrentDistance — drives the spawn-rate ramp from _spawnRateStart to _spawnRateEnd.")]
        [SerializeField] private DistanceTracker _distanceTracker;

        [Tooltip("Parent for instantiated meteorites. Should be the WallConveyor so meteorites inherit the world scroll via transform hierarchy.")]
        [SerializeField] private Transform _meteoritesParent;

        [Tooltip("Distance (world units) at which the spawn rate hits _spawnRateEnd. Equivalent to the per-level target distance.")]
        [SerializeField] private float _levelTargetDistance = 1000f;

        [Header("Spawn rate (meteorites / second)")]
        [Tooltip("Spawn rate at the start of the level (progress = 0).")]
        [SerializeField] private float _spawnRateStart = 0.5f;

        [Tooltip("Spawn rate at the end of the level (progress = 1). Interpolated linearly with distance.")]
        [SerializeField] private float _spawnRateEnd = 4f;

        [Header("Spawn position")]
        [Tooltip("World Y at which meteorites appear. Should sit safely above the camera so the spawn is invisible.")]
        [SerializeField] private float _spawnLineY = 6f;

        [Tooltip("Minimum world X of the spawn band (corridor left).")]
        [SerializeField] private float _spawnXMin = -3f;

        [Tooltip("Maximum world X of the spawn band (corridor right).")]
        [SerializeField] private float _spawnXMax = 3f;

        [Header("Spawn cone")]
        [Tooltip("Half-angle (degrees) of the cone of launch directions, measured from vertical-down. ±30° is the design default — guarantees meteorites never fly upward or strictly sideways at spawn.")]
        [SerializeField] private float _coneHalfAngleDegrees = 30f;

        [Header("Size distribution (relative weights)")]
        [Tooltip("Relative weight of Small in the size pick. Only the ratio Small : Medium : Large matters, not the absolute values.")]
        [SerializeField] private float _weightSmall = 1f;

        [Tooltip("Relative weight of Medium in the size pick.")]
        [SerializeField] private float _weightMedium = 1f;

        [Tooltip("Relative weight of Large in the size pick.")]
        [SerializeField] private float _weightLarge = 1f;

        [Header("Per-size data")]
        [Tooltip("Small meteorite parameters (speed, rotation, mass, prefab variants).")]
        [SerializeField] private SizeData _smallData;

        [Tooltip("Medium meteorite parameters.")]
        [SerializeField] private SizeData _mediumData;

        [Tooltip("Large meteorite parameters.")]
        [SerializeField] private SizeData _largeData;

        // ─── Events ─────────────────────────────────────────────────
        // Fired by SpawnMeteorite() right after Initialize. Plný publisher
        // pattern — spawner volá Invoke priamo.
        public event Action<MeteoriteSize, Vector2> OnMeteoriteSpawned;

        // Re-emitted from Meteorite.Die() via NotifyDestroyed back-call
        // (Variant A proxy publisher pattern).
        public event Action<MeteoriteSize, Vector2> OnMeteoriteDestroyed;

        // Internal raise mechanism for OnMeteoriteDestroyed — callable only
        // by Meteorite.Die() via the cached back-reference. Internal (not public)
        // because C# events cannot be invoked outside the publishing class;
        // a named internal method preserves that contract.
        internal void NotifyDestroyed(MeteoriteSize size, Vector2 position)
        {
            OnMeteoriteDestroyed?.Invoke(size, position);
        }

        // ────────────────────────────────────────────────────────────

        // Handle to the running spawn coroutine — kept so OnDisable can stop it cleanly.
        private Coroutine _spawnRoutine;

        /// <summary>Starts the spawn loop. Tied to component-enabled state so disabling the GameObject pauses spawning.</summary>
        private void OnEnable()
        {
            _spawnRoutine = StartCoroutine(SpawnLoop());
        }

        /// <summary>Stops the spawn coroutine on disable — prevents orphaned coroutines on scene tear-down or pause toggles.</summary>
        private void OnDisable()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        /// <summary>
        /// Coroutine spawn loop. Each iteration recomputes the current spawn rate from
        /// level progress, waits for the corresponding interval, then spawns one meteorite.
        /// Runs forever until OnDisable stops the coroutine.
        /// </summary>
        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                // Level progress 0..1.
                float progress = Mathf.Clamp01(_distanceTracker.CurrentDistance / _levelTargetDistance);

                // Linearly interpolate spawn rate over the level.
                float currentRate = Mathf.Lerp(_spawnRateStart, _spawnRateEnd, progress);

                // Convert rate (meteorites / sec) to interval (sec / meteorite).
                float interval = 1f / currentRate;

                yield return new WaitForSeconds(interval);

                SpawnMeteorite();
            }
        }

        /// <summary>
        /// Picks a size, draws speed and rotation from the matching SizeData range,
        /// chooses a launch direction in the downward cone, instantiates the prefab,
        /// and hands the runtime state to the Meteorite via Initialize(...).
        /// </summary>
        private void SpawnMeteorite()
        {
            // 1. Pick a size by weight.
            MeteoriteSize size = PickSize();
            SizeData data = GetSizeData(size);

            // 2. Pick a sprite variant.
            GameObject prefab = data.prefabs[Random.Range(0, data.prefabs.Length)];

            // 3. Spawn position: random x in the corridor band, fixed y above screen.
            float spawnX = Random.Range(_spawnXMin, _spawnXMax);
            Vector2 spawnPos = new Vector2(spawnX, _spawnLineY);

            // 4. Random starting sprite rotation (uniform 0–360°), so identical
            //    sprites don't look identical at spawn.
            float startAngle = Random.Range(0f, 360f);

            // 5. Velocity vector.
            // Angle offset δ uniform within ±_coneHalfAngleDegrees from vertical-down.
            // Direction = (sin δ, -cos δ) — unit vector for "cone around down".
            // Magnitude = uniform in [minSpeed, maxSpeed].
            float randomDelta = Random.Range(-_coneHalfAngleDegrees, _coneHalfAngleDegrees);
            float randomDeltaRad = Mathf.Deg2Rad * randomDelta;
            Vector2 direction = new Vector2(Mathf.Sin(randomDeltaRad), -Mathf.Cos(randomDeltaRad));
            float speed = Random.Range(data.minSpeed, data.maxSpeed);
            Vector2 velocity = direction * speed;

            // 6. Signed rotation rate: magnitude from the range, sign 50/50.
            float rotMagnitude = Random.Range(data.minRotationDegPerSec, data.maxRotationDegPerSec);
            float rotSign = Random.value < 0.5f ? -1f : 1f;
            float signedRotation = rotMagnitude * rotSign;

            // 7. Instantiate and parent under _meteoritesParent (WallConveyor) so the
            //    meteorite inherits the world scroll, then push the runtime state into
            //    the Meteorite component.
            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0f, 0f, startAngle), _meteoritesParent);
            Meteorite meteorite = obj.GetComponent<Meteorite>();
            meteorite.Initialize(velocity, signedRotation, data.mass, size, this);

            // Invariant #5: Initialize → Invoke (subscriber vidí plne inicializovaný meteorit).
            OnMeteoriteSpawned?.Invoke(size, spawnPos);

        }

        /// <summary>Picks a size using the configured relative weights. Higher weight = more likely.</summary>
        private MeteoriteSize PickSize()
        {
            float total = _weightSmall + _weightMedium + _weightLarge;
            float roll = Random.Range(0f, total);
            if (roll < _weightSmall) return MeteoriteSize.Small;
            if (roll < _weightSmall + _weightMedium) return MeteoriteSize.Medium;
            return MeteoriteSize.Large;
        }

        /// <summary>Returns the SizeData block matching the given size enum.</summary>
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
