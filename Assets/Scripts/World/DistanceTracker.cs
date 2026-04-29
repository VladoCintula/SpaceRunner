using System;
using UnityEngine;

/// <summary>
/// Sleduje preletenú vertikálnu vzdialenos hráèa.
///
/// Pripoèítavanie: cos(uhol_lode) × v_max × deltaTime — uhol a rıchlos pull-uje
/// z PlayerMovement (kontinuálny stav, pull pattern).
///
/// Publishuje event OnMilestoneReached pri kadom prekroèení milestone-u
/// (napr. kadıch 100 m). Subscriber-i (HUD, neskôr Audio, Achievements)
/// reagujú na bod v èase (observer pattern).
///
/// Detail dizajnového rozhodnutia v 21.01.01 Koncept.md.
/// </summary>
public class DistanceTracker : MonoBehaviour
{
    [Header("Závislosti")]
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Parametre")]
    [Tooltip("Vzdialenos medzi milestone-ami (world units / metre).")]
    [SerializeField] private int _milestoneInterval = 100;

    /// <summary>Aktuálna preletená vzdialenos v leveli (world units).</summary>
    public float CurrentDistance { get; private set; }

    // Deklaracia public event - hodnota dosiahnuteho milestone v metroch (napr. 100, 200, 300, ...)
    public event Action<int> OnMilestoneReached;

    // Lokálny stav pre detekciu prekroèenia milestone-u.
    // Drí "èíslo posledného hláseného milestone-u" (0 = iadny, 1 = prvı, 2 = druhı, ...).
    private int _lastMilestoneReported = 0;

    void Update()
    {
        // 1. Pripoèítaj vzdialenos za tento frame.
        float angle = _playerMovement.CurrentAngleRadians;
        float maxSpeed = _playerMovement.MaxSpeed;
        CurrentDistance += Mathf.Cos(angle) * maxSpeed * Time.deltaTime;

        // 2. Detekuj prekroèenie milestone-u a publikuj event.
        int currentMilestoneNumber = (int)(CurrentDistance / _milestoneInterval);
        if (currentMilestoneNumber > _lastMilestoneReported)
        {
            _lastMilestoneReported = currentMilestoneNumber;
            OnMilestoneReached?.Invoke(currentMilestoneNumber * _milestoneInterval);
        }
    }
}