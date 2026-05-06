using UnityEngine;
using SpaceRunner.Player;

namespace SpaceRunner.World
{
    /// <summary>
    /// Scrolls a single wall segment downward at a speed derived from the player's movement.
    /// Pulls MaxSpeed and CurrentAngleRadians from PlayerMovement every frame.
    /// Destroys the GameObject when it falls below the despawn line.
    /// </summary>
    public class WallScroller : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Source of scroll speed (read each frame). Set in Inspector for scene-placed instances; injected via Initialize() for spawner-instantiated walls.")]
        [SerializeField] private PlayerMovement _player;

        [Header("Calibration")]
        [Tooltip("Y-coordinate (world space) below which the wall destroys itself.")]
        [SerializeField] private float _despawnY = -6f;

        /// <summary>
        /// Called by WallSpawner after Instantiate to pass the scene-specific PlayerMovement
        /// reference (which a prefab cannot hold). Manually placed instances skip this and
        /// rely on the Inspector value.
        /// </summary>
        public void Initialize(PlayerMovement player)
        {
            _player = player;
        }

        private void Update()
        {
            // Calculate scroll speed for this frame.
            float scrollSpeed = Mathf.Cos(_player.CurrentAngleRadians) * _player.MaxSpeed;

            // Move the transform down by (scrollSpeed * Time.deltaTime).
            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // Despawn when below the despawn line.
            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }
        }
    }
}