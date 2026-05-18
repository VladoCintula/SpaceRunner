using UnityEngine;
using SpaceRunner.Player;

namespace SpaceRunner.World
{
    /// <summary>
    /// Single GameObject in the scene that acts as the parent for all wall segments
    /// (both left and right sides) and any other scroll-aware entities (currently meteorites).
    /// Scrolls itself downward each frame at a speed derived from PlayerMovement;
    /// children inherit this motion automatically via Unity's transform hierarchy, so
    /// they move in lockstep regardless of when their own Update was first scheduled.
    ///
    /// Replaces the per-wall scrolling logic that previously lived in WallScroller —
    /// the change fixed pixel gaps caused by Unity's Instantiate-skip-Update timing
    /// (see Devlog 2026-05-09 and master Architektúra principle 8).
    /// </summary>
    public class WallConveyor : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Source of CurrentAngleRadians and MaxSpeed (pulled every frame). Scroll speed = cos(angle) * MaxSpeed.")]
        [SerializeField] private PlayerMovement _player;

        private void Update()
        {
            // Scroll speed for this frame: world moves down as fast as the player would
            // move up if the ship weren't y-fixed (cos-projection of v_max onto vertical).
            float scrollSpeed = Mathf.Cos(_player.CurrentAngleRadians) * _player.MaxSpeed;

            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
        }
    }
}
