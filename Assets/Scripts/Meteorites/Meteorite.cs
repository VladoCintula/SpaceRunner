using UnityEngine;

namespace SpaceRunner.Meteorites
{
    public class Meteorite : MonoBehaviour
    {
        [SerializeField] private float _despawnY = -10f;

        private Vector2 _velocity; // vektor definujúci kam sa má meteorit pohnúť
        private float _rotationDegPerSecond;  // signed: + = CCW, - = CW (set-uje spawner)

        /// <summary>
        /// Inicializácia po Instantiate. Spawner volá tesne po vytvorení meteoritu.
        /// </summary>
        public void Initialize(Vector2 velocity, float rotationDegPerSecond)
        {
            _velocity = velocity;
            _rotationDegPerSecond = rotationDegPerSecond;
        }

        private void Update()
        {
            // Pohyb pozície podľa velocity vektora
            transform.position += (Vector3)_velocity * Time.deltaTime;

            // 2. Vizuálna rotácia okolo z-osi (Z, lebo 2D hra v xy rovine).
            //    Znamienko v _rotationDegPerSecond určuje smer CW/CCW.
            transform.Rotate(0f, 0f, _rotationDegPerSecond * Time.deltaTime);

            // 3. Despawn pri opustení obrazovky smerom dole.
            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }
        }
    }
}