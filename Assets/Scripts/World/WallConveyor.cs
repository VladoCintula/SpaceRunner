using UnityEngine;
using SpaceRunner.Player;

namespace SpaceRunner.World
{
    /// <summary>
    /// Single GameObject in the scene that acts as the parent for all wall segments
    /// (both left and right sides). Scrolls itself downward each frame at a speed
    /// derived from PlayerMovement; child walls inherit this motion automatically
    /// via Unity's transform hierarchy, so they move in lockstep regardless of
    /// when their own Update was first scheduled.
    ///
    /// Replaces the per-wall scrolling logic that previously lived in WallScroller.
    /// </summary>
    public class WallConveyor : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Source of CurrentAngleRadians and MaxSpeed (pulled every frame).")]
        [SerializeField] private PlayerMovement _player;

        private void Update()
        {
            // Calculate scroll speed for this frame.
            float scrollSpeed = Mathf.Cos(_player.CurrentAngleRadians) * _player.MaxSpeed;

            // Move the transform down by (scrollSpeed * Time.deltaTime).
            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
        }
    }
}