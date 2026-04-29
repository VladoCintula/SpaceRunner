using UnityEngine;

/// <summary>
/// Riadi pohyb a natočenie hráčovej lode podľa pozície kurzora myši.
///
/// Pohybový model: konštantná celková rýchlosť v_max, rozkladá sa cez sin(uhol)
/// na horizontálny pohyb. Lod má fixnú y-pozíciu, vertikálna zložka pohybu sa
/// neaplikuje na pozíciu, ale je dostupná pre iné systémy cez CurrentAngleRadians
/// (napr. DistanceTracker pre cos-projekciu progresu v leveli).
///
/// Detail dizajnového rozhodnutia v 21.01.01 Koncept.md, sekcia "Pohybový model lode".
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Pohybové parametre")]
    [Tooltip("Maximálna celková rýchlosť lode (world units / sec). Horizontálny pohyb = v_max × sin(uhol).")]
    [SerializeField] private float _maxSpeed = 5f;

    /// <summary>
    /// Aktuálny uhol natočenia lode od vertikály, v radiánoch.
    /// Rozsah: -π/2 (90° doľava) až +π/2 (90° doprava). 0 = priamo hore.
    /// Vystavené pull-pattern pre iné systémy (DistanceTracker, neskôr Weapon).
    /// </summary>
    public float CurrentAngleRadians { get; private set; }

    /// <summary>Maximálna celková rýchlosť — pre DistanceTracker (cos-projekcia progresu).</summary>
    public float MaxSpeed => _maxSpeed;

    private Camera _mainCamera;

    void Awake()
    {
        // Camera.main je relatívne pomalý lookup (cez tag) — cache-ujeme raz.
        _mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. Kurzor v world súradniciach.
        // ScreenToWorldPoint vyžaduje aj z-súradnicu (vzdialenosť od kamery).
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -_mainCamera.transform.position.z;
        Vector3 cursorWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);

        // 2. Cap kurzora pod loďou.
        // Z dizajnu: ak je kurzor pod úrovňou lode, pre výpočet ho považujeme za
        // ležiaci na úrovni lode. Tým sa uhol nedostane nad ±90° v abs. hodnote.
        if (cursorWorld.y < transform.position.y)
        {
            cursorWorld.y = transform.position.y;
        }

        // 3. Vektor od lode ku kurzoru.
        Vector2 toCursor = (Vector2)(cursorWorld - transform.position);

        // 3b. Mŕtva zóna v okolí lode.
        // Pri kurzore príliš blízko (alebo presne pri) lode by Atan2(0, 0) bol nedefinovaný
        // a pri kurzore pod loďou (po cape y → lod.y) by Atan2 skákal medzi ±π/2,
        // čo vedie k bang-bang oscilácii. V deadzone proste nehýbeme.
        const float DEAD_ZONE_RADIUS = 0.3f;
        if (toCursor.sqrMagnitude < DEAD_ZONE_RADIUS * DEAD_ZONE_RADIUS)
        {
            return;
        }

        // 4. Uhol od vertikály v radiánoch.
        // Mathf.Atan2(x, y) — prehodené poradie argumentov vracia uhol od +Y osi.
        // Kladný = kurzor vpravo, záporný = kurzor vľavo.
        CurrentAngleRadians = Mathf.Atan2(toCursor.x, toCursor.y);

        // 5. Aplikuj rotáciu na loď.
        // Unity 2D rotácia okolo Z je counter-clockwise (kladný uhol = vizuálne doľava).
        // Náš uhol je clockwise-from-vertical, preto znamienko mínus.
        float angleDegrees = -CurrentAngleRadians * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        // 6. Aplikuj horizontálny pohyb.
        // horizontálna_rýchlosť = v_max × sin(uhol).
        // Time.deltaTime → frame-rate independent pohyb.
        float deltaX = _maxSpeed * Mathf.Sin(CurrentAngleRadians) * Time.deltaTime;
        Vector3 pos = transform.position;
        pos.x += deltaX;
        transform.position = pos;
    }
}