using UnityEngine;
using SpaceRunner.Meteorites;

namespace SpaceRunner.Weapons
{
    /// <summary>
    /// A single player projectile. Carries its velocity vector, moves itself each frame
    /// on a straight line (motion paradigm A, mirroring Meteorite), despawns when it
    /// leaves the screen upward (above _despawnY), and damages the first meteorite it
    /// hits before despawning (non-pierce — design decision F5b).
    ///
    /// Scroll inheritance comes from the parent (WallConveyor) set at Instantiate time;
    /// this Update only drives the projectile's own upward trajectory. Walls are filtered
    /// out by the layer collision matrix (Projectile × Wall disabled), so a hit can only
    /// ever be a meteorite — but we still guard with a GetComponent check (F5a pass-through).
    ///
    /// Design rationale: Devlog 2026-06-20 (Streľba, F1–F7); 21.01.01 Koncept.md, section "Streľba".
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Damage dealt to a meteorite on hit. Always 1 by design (21.01.01 Koncept, Streľba).")]
        [SerializeField] private int _damage = 1;

        [Tooltip("World Y above which the projectile destroys itself. Mirrors Meteorite._despawnYTop — upper off-screen edge.")]
        [SerializeField] private float _despawnY;

        // Written once at Initialize; the projectile flies straight along it.
        private Vector2 _velocity;

        /// <summary>
        /// Post-Instantiate initializer. Called by ProjectileSpawner immediately after the
        /// GameObject is created. Stores the velocity and orients the sprite along the flight
        /// direction (sprite default points +Y).
        /// </summary>
        /// <param name="velocity">Initial velocity vector (direction * speed).</param>
        public void Initialize(Vector2 velocity)
        {
            _velocity = velocity;

            // Orient the sprite along the flight direction. Angle from vertical (+Y) is
            // Atan2(x, y); Unity 2D Z-rotation is CCW while our angle is CW-from-vertical,
            // hence the minus sign — same convention as PlayerMovement (verify in Play mode).
            float angleDeg = -Mathf.Atan2(_velocity.x, _velocity.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        private void Update()
        {
            // Straight-line motion along the current velocity vector (parent handles scroll).
            transform.position += (Vector3)(_velocity * Time.deltaTime);

            // Despawn once the projectile has left the screen upward.
            if (transform.position.y > _despawnY)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only react to meteorites. Walls are already excluded by the layer matrix,
            // but the GetComponent guard keeps the contact handling explicit and safe.
            Meteorite meteorite = other.GetComponent<Meteorite>();
            if (meteorite == null)
                return;

            // Non-pierce (F5b): deal damage once, then despawn on the first hit.
            meteorite.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
