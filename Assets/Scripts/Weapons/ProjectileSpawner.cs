using UnityEngine;
using SpaceRunner.Player;

namespace SpaceRunner.Weapons
{
    /// <summary>
    /// Component on the ship that owns the whole shooting path: reads the fire input,
    /// enforces the cadence cooldown, and spawns projectiles from the muzzle in the
    /// ship's current aiming direction (decision F1 — one class holds input + cadence +
    /// spawn, mirroring how MeteoriteSpawner holds everything for its domain).
    ///
    /// Projectiles are parented to the same scroll-aware parent as meteorites
    /// (WallConveyor) so they share the world reference frame; their upward motion is
    /// their own (Projectile.Update), the parent only contributes scroll (decision F3,
    /// principle #8). No events yet — there is no Audio subscriber, so this is only a
    /// latent proxy publisher ("príde subscriber, príde event").
    ///
    /// Design rationale: Devlog 2026-06-20 (Streľba, F1–F7); 21.01.01 Koncept.md, section "Streľba".
    /// </summary>
    public class ProjectileSpawner : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Source of CurrentAngleRadians — the aiming direction projectiles are fired along.")]
        [SerializeField] private PlayerMovement _playerMovement;

        [Tooltip("Spawn origin — empty child at the ship's nose.")]
        [SerializeField] private Transform _muzzle;

        [Tooltip("Projectile prefab to instantiate per shot.")]
        [SerializeField] private Projectile _projectilePrefab;

        [Tooltip("Parent for spawned projectiles. Should be the WallConveyor so they inherit the world scroll (same frame as meteorites).")]
        [SerializeField] private Transform _worldParent;

        [Header("Shooting parameters")]
        [Tooltip("Seconds between shots. Default 1 shot/sec (21.01.01 Koncept, Streľba).")]
        [SerializeField] private float _fireInterval = 1f;

        [Tooltip("Projectile speed (world units / sec). ~5× ship speed (21.01.01 Koncept, Streľba).")]
        [SerializeField] private float _projectileSpeed = 25f;

        /// <summary>
        /// Cadence hook for the future PowerUps domain (decision F7). No PU coupling yet —
        /// a clean property over _fireInterval so a fire-rate power-up can shorten the interval.
        /// </summary>
        public float FireInterval
        {
            get => _fireInterval;
            set => _fireInterval = value;
        }

        // Counts down to the next allowed shot. Initialized to 0 so the very first shot is
        // available immediately — no wait for the first cooldown (21.01.01 Koncept, Streľba).
        private float _cooldown = 0f;

        private void Update()
        {
            _cooldown -= Time.deltaTime;

            // Old Input Manager (consistent with PlayerMovement). GetMouseButton(0) is true
            // while held, so holding fires at the cadence; clicks during cooldown are ignored.
            if (Input.GetMouseButton(0) && _cooldown <= 0f)
            {
                Fire();
                _cooldown = _fireInterval;
            }
        }

        private void Fire()
        {
            // Direction from the ship's aim angle: θ=0 is up, +θ is right, matching the
            // (sin θ, cos θ) schema (CurrentAngleRadians = Atan2(toCursor.x, toCursor.y)).
            float t = _playerMovement.CurrentAngleRadians;
            Vector2 dir = new Vector2(Mathf.Sin(t), Mathf.Cos(t));

            // Parent to _worldParent (WallConveyor) for scroll inheritance — same reference
            // frame as meteorites (F3); Projectile.Update drives the upward trajectory.
            Projectile projectile = Instantiate(_projectilePrefab, _muzzle.position, Quaternion.identity, _worldParent);
            projectile.Initialize(dir * _projectileSpeed);
        }
    }
}
