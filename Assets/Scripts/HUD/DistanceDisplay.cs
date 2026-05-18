using UnityEngine;
using TMPro;
using SpaceRunner.World;

namespace SpaceRunner.HUD
{
    /// <summary>
    /// Renders the player's current flown distance in the level.
    ///
    /// Uses both communication patterns simultaneously and on purpose:
    ///   - Pull: in Update() reads distanceTracker.CurrentDistance for the continuous readout.
    ///   - Observer: subscribed to OnMilestoneReached for the per-milestone visual flash.
    ///
    /// Continuous value → pull. Point-in-time event → observer. Mixing both in one class
    /// is fine when the data has both natures.
    ///
    /// Design rationale: master Architektúra, principle 2 "Pull for continuous state, observer for point in time".
    /// </summary>
    public class DistanceDisplay : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Source of CurrentDistance (pulled every frame) and OnMilestoneReached (subscribed in OnEnable).")]
        [SerializeField] private DistanceTracker _distanceTracker;

        [Tooltip("TMP label that displays the distance readout. Color is also driven by this script (red during a flash, white otherwise).")]
        [SerializeField] private TextMeshProUGUI _distanceText;

        [Header("Flash effect")]
        [Tooltip("Duration of the red flash on the readout when a milestone is crossed (seconds).")]
        [SerializeField] private float _flashDurationSeconds = 0.2f;

        // Remaining flash time in seconds. 0 = not flashing.
        private float _remainingFlashTime = 0f;

        /// <summary>Subscribe to milestone events. Paired with OnDisable so the handler tracks the component's enabled state.</summary>
        private void OnEnable()
        {
            _distanceTracker.OnMilestoneReached += PerformFlash;
        }

        /// <summary>Symmetric unsubscribe. Must use the same method reference as the OnEnable += for it to take effect.</summary>
        private void OnDisable()
        {
            _distanceTracker.OnMilestoneReached -= PerformFlash;
        }

        /// <summary>
        /// Milestone event handler. Starts a flash by resetting the remaining flash time;
        /// Update() consumes it down to zero and restores the white color.
        /// </summary>
        /// <param name="_milestone">Milestone value in metres. Not used here — only the trigger matters.</param>
        private void PerformFlash(int _milestone)
        {
            _remainingFlashTime = _flashDurationSeconds;
        }

        private void Update()
        {
            // 1. Pull: refresh the readout from the current distance.
            // (int) rounds down to whole metres for a readable display.
            int distanceMeters = (int)_distanceTracker.CurrentDistance;
            _distanceText.text = $"{distanceMeters} m";

            // 2. Visual flash: red while a flash is active, white otherwise.
            if (_remainingFlashTime > 0f)
            {
                _distanceText.color = Color.red;
                _remainingFlashTime -= Time.deltaTime;
            }
            else
            {
                _distanceText.color = Color.white;
            }
        }
    }
}
