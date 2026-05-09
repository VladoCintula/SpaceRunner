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
    /// the scroll and produce 2× speed.
    /// </summary>
    public class WallSegment : MonoBehaviour
    {
        [Tooltip("World Y below which the segment destroys itself.")]
        [SerializeField] private float _despawnY = -6f;

        private void Update()
        {

            // Despawn wall segment when below the despawn line.
            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }
        }
    }
}