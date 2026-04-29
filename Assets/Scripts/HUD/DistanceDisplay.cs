using UnityEngine;
using TMPro;

/// <summary>
/// Zobrazuje aktuálnu preletenú vzdialenos hráèa v leveli.
///
/// Pouíva dva mechanizmy súèasne:
///   - Pull: v Update() èíta distanceTracker.CurrentDistance pre kontinuálny readout.
///   - Observer: prihlásenı na OnMilestoneReached pre vizuálny flash pri milestone.
///
/// Toto je vedomé pouitie oboch patternov v jednej triede — kontinuálna hodnota
/// patrí pull-u, bod v èase patrí eventu.
/// </summary>
public class DistanceDisplay : MonoBehaviour
{
    [Header("Závislosti")]
    [SerializeField] private DistanceTracker _distanceTracker;
    [SerializeField] private TextMeshProUGUI _distanceText;

    [Header("Flash effect")]
    [Tooltip("Dåka vizuálneho flash-u pri dosiahnutí milestone-u (sekundy).")]
    [SerializeField] private float _flashDurationSeconds = 0.2f;

    // Lokálny stav: ako dlho ešte trvá flash (sekundy). 0 = u nie je flash.
    private float _remainingFlashTime = 0f;


    private void OnEnable()
    {
        _distanceTracker.OnMilestoneReached += PerformFlash;
    }

    private void OnDisable()
    {
        _distanceTracker.OnMilestoneReached -= PerformFlash;
    }


    // Handler metóda, ktorá reaguje na OnMilestoneReached.
    // Logika: nastav remainingFlashTime = flashDurationSeconds.
    // Tım spustíš flash, ktorı Update() postupne dokonèí.
    void PerformFlash(int _milestone)
    {
        _remainingFlashTime = _flashDurationSeconds;
    }


    void Update()
    {
        // 1. Pull: aktualizuj text aktuálnou vzdialenosou.
        // (int) zaokrúhli na celé metre pre èitate¾nı readout.
        int distanceMeters = (int)_distanceTracker.CurrentDistance;
        _distanceText.text = $"{distanceMeters} m";

        // 2. Vizuálny flash: ak práve flashujeme, aplikuj èervenú farbu, inak bielu.
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