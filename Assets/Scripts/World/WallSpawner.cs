using System.Collections.Generic;
using UnityEngine;
using SpaceRunner.Player;

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
        [Tooltip("Wall prefabs to choose from. Each must have a WallScroller component.")]
        [SerializeField] private GameObject[] _wallPrefabs;

        [Header("Side configuration")]
        [Tooltip("If true, instantiated walls are flipped horizontally (localScale.x = -1). Use for the right corridor wall.")]
        [SerializeField] private bool _flipHorizontally;

        [Header("Dependencies")]
        [Tooltip("Player reference passed to each new wall via WallScroller.Initialize.")]
        [SerializeField] private PlayerMovement _player;

        // Bag shuffle state
        private readonly List<int> _bag = new List<int>();
        private int _lastUsedIndex = -1;

        // Position-based trigger state
        private GameObject _lastWall;

        private void Update()
        {
            if (ShouldSpawnNext())
            {
                SpawnNext();
            }
        }

        /// <summary>
        /// True when the next wall should spawn 
        /// </summary>
        private bool ShouldSpawnNext()
        {
            if (_lastWall == null)
                return true;

            // Trigger: top of last has descended below spawn line
            return _lastWall.transform.position.y < transform.position.y;
        }
        private void SpawnNext()
        {
            int wallIndex = DrawFromBag();
            GameObject prefab = _wallPrefabs[wallIndex];

            // Temporary spawn position — we'll reposition once we know the real height.
            GameObject newWall = Instantiate(prefab, transform.position, Quaternion.identity);

            if (_flipHorizontally)
            {
                newWall.transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            if (_lastWall != null)
            {
                float heightNew = newWall.GetComponent<SpriteRenderer>().bounds.size.y;
                newWall.transform.position = new Vector3(
                    transform.position.x,
                    _lastWall.transform.position.y + heightNew,
                    0f
                );
            }

            newWall.GetComponent<WallScroller>().Initialize(_player);
            _lastWall = newWall;
        }


        private Vector3 ComputeSpawnPosition(GameObject prefab)
        {
            if (_lastWall == null)
                return transform.position;

            // Chain grows upward: top of new = top of last + height of new
            float heightNew = prefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
            return new Vector3(
                transform.position.x,
                _lastWall.transform.position.y + heightNew,
                0f
            );
        }


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

        private void RefillBag()
        {
            _bag.Clear();
            for (int i = 0; i < _wallPrefabs.Length; i++)
                _bag.Add(i);
        }

        // Compute the bottom edge of _lastWall in world space
        private float GetBottomOfLast()
        {
            return _lastWall.transform.position.y - _lastWall.GetComponent<SpriteRenderer>().bounds.size.y;
        }
    }
}