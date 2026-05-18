using UnityEngine;

namespace SpaceRunner.Meteorites
{
    public class Meteorite : MonoBehaviour
    {
        [SerializeField] private float _despawnY = -10f;

        private Vector2 _velocity; // vektor definujúci kam sa má meteorit pohnúť
        private float _rotationDegPerSecond; // signed: + = CCW, - = CW (set-uje spawner)
        private float _mass; // runtime-injected zo SizeData pri spawne (Q2)

        /// <summary>
        /// Read-only okno na velocity pre resolver kolízie druhého meteoritu.
        /// </summary>
        public Vector2 Velocity => _velocity;

        /// <summary>
        /// Read-only okno na hmotnosť pre vzorec elastickej kolízie. Single source je SizeData.
        /// </summary>
        public float Mass => _mass;

        /// <summary>
        /// Inicializácia po Instantiate. Spawner volá tesne po vytvorení meteoritu.
        /// </summary>
        public void Initialize(Vector2 velocity, float rotationDegPerSecond, float mass)
        {
            _velocity = velocity;
            _rotationDegPerSecond = rotationDegPerSecond;
            _mass = mass;
        }

        /// <summary>
        /// Prepíše velocity. Volá víťazný resolver kolízie (ten s nižším InstanceID),
        /// keď zapisuje výsledok elastickej kolízie aj druhému meteoritu.
        /// </summary>
        public void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }

        private void Update()
        {
            // Pohyb pozície podľa velocity vektora
            transform.position += (Vector3)_velocity * Time.deltaTime;

            // 2. Vizuálna rotácia okolo z-osi (Z, lebo 2D hra v xy rovine).
            // Znamienko v _rotationDegPerSecond určuje smer CW/CCW.
            transform.Rotate(0f, 0f, _rotationDegPerSecond * Time.deltaTime);

            // 3. Despawn pri opustení obrazovky smerom dole.
            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Kolízia meteorit↔meteorit. Oba meteority fire-nú tento handler v tom istom
        /// physics kroku; ID guard zabezpečí, že pár vyrieši len jeden z nich.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            Meteorite otherMeteorite = other.GetComponent<Meteorite>();
            if (otherMeteorite == null)
            {
                // Nie je meteorit (stena rieši až časť 3; Wall×Meteorite je v matrixe OFF).
                return;
            }

            // ID guard (Q1): pár rieši len meteorit s nižším InstanceID, druhý skončí.
            if (GetInstanceID() > otherMeteorite.GetInstanceID())
            {
                return;
            }

            // Vstupy pre vzorec — čítame OBA stavy pred akýmkoľvek zápisom.
            Vector2 posA = transform.position;
            Vector2 posB = otherMeteorite.transform.position;
            Vector2 vA = _velocity;
            Vector2 vB = otherMeteorite.Velocity;
            float mA = _mass;
            float mB = otherMeteorite.Mass;

            // ──────────────────────────────────────────────────────────────
            // TODO 1 (tvoja zóna — Stop & Learn 15.5., krok 1):
            //   kolízna normála n pre kruh-kruh.
            Vector2 n = (posB - posA).normalized;
            // ──────────────────────────────────────────────────────────────

            // ──────────────────────────────────────────────────────────────
            // TODO 2 (tvoja zóna — Stop & Learn 15.5., kroky 2–6):
            //   dekompozícia vA, vB na normálovú + tangenciálnu zložku,
            //   tangenciálne sa nemenia, NORMÁLOVÉ sa prerozdelia podľa
            //   hmotností (všeobecný vzorec pre rôzne mA, mB), rekompozícia.

            float aN = vA.x * n.x + vA.y * n.y;   // alebo Vector2.Dot(vA, n)
            float bN = vB.x * n.x + vB.y * n.y;

            Vector2 vA_v_n = aN * n;
            Vector2 vA_v_t = vA - vA_v_n;

            Vector2 vB_v_n = bN * n;
            Vector2 vB_v_t = vB - vB_v_n;


            Vector2 vA_new = ((mA - mB) * vA_v_n + 2 * mB * vB_v_n) / (mA + mB) + vA_v_t;
            Vector2 vB_new = ((mB - mA) * vB_v_n + 2 * mA * vA_v_n) / (mA + mB) + vB_v_t;
            // ──────────────────────────────────────────────────────────────

            // HOOK Q3 (Možnosť B — TERAZ NEAPLIKUJEME):
            //   ak prototyp ukáže časté lietanie hore → sem clamp y-zložky
            //   + renormalizácia magnitúdy. Možnosť A = akceptujeme realistickú fyziku.

            _velocity = vA_new;
            otherMeteorite.SetVelocity(vB_new);
        }
    }
}