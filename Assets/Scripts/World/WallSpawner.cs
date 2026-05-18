using System.Collections.Generic;
using UnityEngine;

namespace SpaceRunner.World
{
    /// <summary>
    /// Spawns wall segments at this transform's position and chains them so they form
    /// a continuous corridor edge. Each wall scrolls itself; this spawner only decides
    /// when and which prefab to instantiate.
    ///
    /// Two instances of this class live in the scene: one for the left side, one for
    /// the right (with _flipHorizontally = true). They run independent bag shuffles,
    /// which produces emergent asymmetry across the corridor.
    /// </summary>
    public class WallSpawner : MonoBehaviour
    {
        [Header("Pool")]
        [Tooltip("Wall prefabs to choose from. Each must have a WallSegment component and a SpriteRenderer.")]
        [SerializeField] private GameObject[] _wallPrefabs;

        [Header("Side configuration")]
        [Tooltip("If true, instantiated walls are flipped horizontally (localScale.x = -1). Use for the right corridor wall.")]
        [SerializeField] private bool _flipHorizontally;

        [Header("Dependencies")]
        [Tooltip("Conveyor that all spawned walls are parented to. Scroll movement is inherited via transform hierarchy.")]
        [SerializeField] private WallConveyor _conveyor;

        // Bag shuffle state: indices into _wallPrefabs, drained one draw at a time
        // and refilled when empty. Guarantees each prefab appears once per round.
        private readonly List<int> _bag = new List<int>();

        // Index of the most recently drawn prefab. Used to suppress an immediate repeat
        // when a fresh bag would otherwise let the same prefab spawn twice in a row.
        private int _lastUsedIndex = -1;

        // Reference to the most recently spawned wall — drives both the position-based
        // spawn trigger and the chain growing upward (new wall's bottom = old wall's top).
        private GameObject _lastWall;

        private void Update()
        {
            if (ShouldSpawnNext())
            {
                SpawnNext();
            }
        }

        /// <summary>
        /// True when the next wall should spawn — either there is no previous wall yet,
        /// or the last wall has descended below this spawner's y-position.
        /// </summary>
        private bool ShouldSpawnNext()
        {
            if (_lastWall == null)
                return true;

            // Trigger: top of last has descended below spawn line.
            return _lastWall.transform.position.y < transform.position.y;
        }

        /// <summary>
        /// Draws the next prefab from the bag, instantiates it parented to the conveyor,
        /// stacks it on top of the previous wall (so the chain grows upward without gaps),
        /// and updates _lastWall.
        /// </summary>
        private void SpawnNext()
        {
            int wallIndex = DrawFromBag();
            GameObject prefab = _wallPrefabs[wallIndex];

            GameObject newWall = Instantiate(prefab, transform.position, Quaternion.identity, _conveyor.transform);

            if (_flipHorizontally)
            {
                newWall.transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            if (_lastWall != null)
            {
                // Stack the new wall directly on top of the last one — anchor is the centre,
                // so shift up by the new wall's full height.
                float heightNew = newWall.GetComponent<SpriteRenderer>().bounds.size.y;
                float lastY = _lastWall.transform.position.y;
                float newY = lastY + heightNew;

                newWall.transform.position = new Vector3(transform.position.x, newY, 0f);
            }

            _lastWall = newWall;
        }

        /// <summary>
        /// Computes the spawn position of a new wall stacked on top of _lastWall.
        /// Currently unused — SpawnNext() inlines the same calculation.
        /// </summary>
        private Vector3 ComputeSpawnPosition(GameObject prefab)
        {
            if (_lastWall == null)
                return transform.position;

            // Chain grows upward: top of new = top of last + height of new.
            float heightNew = prefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
            return new Vector3(
                transform.position.x,
                _lastWall.transform.position.y + heightNew,
                0f
            );
        }

        /// <summary>
        /// Draws one prefab index from the bag, avoiding an immediate repeat of the
        /// previous draw (unless only one prefab remains in the bag). Refills the bag
        /// when it runs empty.
        /// </summary>
        private int DrawFromBag()
        {
            if (_bag.Count == 0)
                RefillBag();

            int idx;
            int wallIndex;
            do
            {
                idx = Random.Range(0, _bag.Count);
                wallIndex = _bag[idx];
            } while (_bag.Count > 1 && wallIndex == _lastUsedIndex);

            _bag.RemoveAt(idx);
            _lastUsedIndex = wallIndex;
            return wallIndex;
        }

        /// <summary>Refills the bag with one entry per wall prefab — the start of a new shuffle round.</summary>
        private void RefillBag()
        {
            _bag.Clear();
            for (int i = 0; i < _wallPrefabs.Length; i++)
                _bag.Add(i);
        }

        /// <summary>World-space Y of the bottom edge of _lastWall. Currently unused.</summary>
        private float GetBottomOfLast()
        {
            return _lastWall.transform.position.y - _lastWall.GetComponent<SpriteRenderer>().bounds.size.y;
        }
    }
}
