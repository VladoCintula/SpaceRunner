using UnityEngine;

namespace SpaceRunner.Meteorites
{
    /// <summary>
    /// Debug komponent pre overenie meteorite eventov v F·ze C.
    /// Subscribe-uje sa na OnMeteoriteSpawned a OnMeteoriteDestroyed
    /// a loguje payload. Kl·vesa X zabÌja najstaröÌ ûij˙ci meteorit
    /// (`FindObjectOfType` vr·ti prv˝ n·jden˝ v scÈne).
    /// TODO: odstr·niù po Session 3 / Weapons domÈne.
    /// </summary>
    public class MeteoriteEventDebugger : MonoBehaviour
    {
        [SerializeField] private MeteoriteSpawner _spawner;
        [SerializeField] private KeyCode _killKey = KeyCode.X;

        private void OnEnable()
        {
            if (_spawner == null)
            {
                Debug.LogError("[MeteoriteEventDebugger] _spawner nepriraden˝ v Inspectore.");
                return;
            }
            _spawner.OnMeteoriteSpawned += HandleSpawned;
            _spawner.OnMeteoriteDestroyed += HandleDestroyed;
        }

        private void OnDisable()
        {
            if (_spawner == null) return;
            _spawner.OnMeteoriteSpawned -= HandleSpawned;
            _spawner.OnMeteoriteDestroyed -= HandleDestroyed;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_killKey))
            {
                Meteorite target = FindObjectOfType<Meteorite>();
                if (target != null)
                {
                    // target.Die();
                    target.TakeDamage(1);
                }
                else
                {
                    Debug.Log("[MeteoriteEventDebugger] éiadny meteorit v scÈne na zabitie.");
                }
            }
        }

        private void HandleSpawned(MeteoriteSpawner.MeteoriteSize size, Vector2 position)
        {
            Debug.Log($"[SPAWNED] {size} @ {position}");
        }

        private void HandleDestroyed(MeteoriteSpawner.MeteoriteSize size, Vector2 position)
        {
            Debug.Log($"[DESTROYED] {size} @ {position}");
        }
    }
}