using UnityEngine;

namespace SpaceRunner.World
{
    /// <summary>
    /// Component on each wall segment prefab. Owns only the despawn lifecycle —
    /// when the segment's world Y drops below _despawnY, it destroys itself.
    ///
    /// Movement is NOT this class's responsibility: the wall is parented to a
    /// WallConveyor which scrolls, and the world position updates automatically
    /// via transform hierarchy. Adding any movement logic here would double-apply
    /// the scroll and produce 2× speed (master Architektúra principle 8 anti-pattern).
    /// </summary>
    public class WallSegment : MonoBehaviour
    {
        [Header("Despawn")]
        [Tooltip("World Y below which the segment destroys itself. Should sit safely off-screen below the camera.")]
        [SerializeField] private float _despawnY = -6f;

        private void Update()
        {
            // Despawn the wall segment once it has scrolled off the bottom of the screen.
            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }
        }
    }
}
