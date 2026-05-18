using System;
using UnityEngine;
using SpaceRunner.Player;

namespace SpaceRunner.World
{
    /// <summary>
    /// Tracks the player's flown vertical distance through the current level.
    ///
    /// Distance accumulation uses cos(ship_angle) * v_max * deltaTime — angle and
    /// max speed are pulled from PlayerMovement every frame (pull pattern for
    /// continuous state).
    ///
    /// Publishes OnMilestoneReached whenever the player crosses a milestone threshold
    /// (e.g. every 100 m). Subscribers (HUD, later Audio, Achievements) react to a
    /// point-in-time event (observer pattern).
    ///
    /// Design rationale: 21.01.01 Koncept.md, section "Y-zložka projekcie a preletená vzdialenosť";
    /// master Architektúra, principle 2 "Pull for continuous state, observer for point in time".
    /// </summary>
    public class DistanceTracker : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Source of CurrentAngleRadians and MaxSpeed (pulled every frame).")]
        [SerializeField] private PlayerMovement _playerMovement;

        [Header("Parameters")]
        [Tooltip("Distance between milestones (world units / metres). Each crossing fires OnMilestoneReached.")]
        [SerializeField] private int _milestoneInterval = 100;

        /// <summary>Current flown distance in the level (world units).</summary>
        public float CurrentDistance { get; private set; }

        /// <summary>Fires once per milestone crossing. Argument: milestone value in metres (100, 200, 300, ...).</summary>
        public event Action<int> OnMilestoneReached;

        // Index of the last reported milestone (0 = none yet, 1 = first, 2 = second, ...).
        // Used to detect threshold crossings without re-firing for the same milestone.
        private int _lastMilestoneReported = 0;

        private void Update()
        {
            // 1. Accumulate distance for this frame.
            float angle = _playerMovement.CurrentAngleRadians;
            float maxSpeed = _playerMovement.MaxSpeed;
            CurrentDistance += Mathf.Cos(angle) * maxSpeed * Time.deltaTime;

            // 2. Detect a milestone crossing and publish.
            int currentMilestoneNumber = (int)(CurrentDistance / _milestoneInterval);
            if (currentMilestoneNumber > _lastMilestoneReported)
            {
                _lastMilestoneReported = currentMilestoneNumber;
                OnMilestoneReached?.Invoke(currentMilestoneNumber * _milestoneInterval);
            }
        }
    }
}
