using UnityEngine;

namespace SpaceRunner.Meteorites
{
    /// <summary>
    /// A single meteorite entity. Carries its velocity, visual rotation rate and mass,
    /// moves itself each frame on a straight line, and resolves elastic 2D collisions
    /// with other meteorites in OnTriggerEnter2D.
    ///
    /// Motion paradigm A — straight line, constant velocity vector between events.
    /// Velocity changes only on events (spawn, collision, future split), never via
    /// continuous gravity or drag. Despawns when its world Y drops below _despawnY.
    ///
    /// Initialized once by MeteoriteSpawner via Initialize(...); SizeData on the
    /// spawner is the single source of mass (see Devlog 2026-05-18, Q2 — Rigidbody2D.mass
    /// is ignored for Kinematic bodies, so we keep our own field).
    ///
    /// Design rationale: 21.01.02 Meteority.md, sections "Pohyb meteoritov" and "Fyzika odrazov".
    /// </summary>
    public class Meteorite : MonoBehaviour
    {
        [Header("Despawn")]
        [Tooltip("World Y below which the meteorite destroys itself. Should sit safely off-screen below the camera.")]
        [SerializeField] private float _despawnY = -10f;
        [SerializeField] private float _despawnYTop = 8f;   // tesne pod spawn líniou 

        [SerializeField] private LayerMask _wallLayer;      // v Inspectore nastav na vrstvu Wall

        private Collider2D _collider;                       // vlastný collider, cache (rozhodnutie Q3)

        // Runtime state, written once at Initialize and then updated by motion / collisions.
        private Vector2 _velocity;
        private float _rotationDegPerSecond; // signed: + = CCW, - = CW
        private float _mass;                 // injected from SizeData at spawn time (Q2)

        private MeteoriteSpawner.MeteoriteSize _size;
        private MeteoriteSpawner _spawner;

        /// <summary>Read-only window onto the velocity, used by the winning side of a meteorite↔meteorite collision resolver.</summary>
        public Vector2 Velocity => _velocity;

        /// <summary>Read-only window onto the mass, used by the elastic collision formula. SizeData on the spawner is the single source.</summary>
        public float Mass => _mass;

        public MeteoriteSpawner.MeteoriteSize Size => _size;

        /// <summary>
        /// Post-Instantiate initializer. Called by MeteoriteSpawner immediately after
        /// the GameObject is created — sets the runtime state that drives motion and
        /// collision response.
        /// </summary>
        /// <param name="velocity">Initial velocity vector (direction * speed).</param>
        /// <param name="rotationDegPerSecond">Signed rotation rate around Z (+ = CCW, - = CW).</param>
        /// <param name="mass">Mass for the elastic collision formula. Pulled from SizeData on the spawner.</param>
        public void Initialize(
            Vector2 velocity,
            float rotationDegPerSecond,
            float mass,
            MeteoriteSpawner.MeteoriteSize size,
            MeteoriteSpawner spawner)
        {
            _velocity = velocity;
            _rotationDegPerSecond = rotationDegPerSecond;
            _mass = mass;
            _size = size;
            _spawner = spawner;
        }

        private void Awake()
        {
            // .Distance voláme každú kolíziu so stenou; GetComponent v handleri by bol
            // zbytočný opakovaný lookup (ten istý dôvod ako cache Camera.main v PlayerMovement).
            _collider = GetComponent<Collider2D>();
        }

        /// <summary>
        /// Overwrites the velocity. Called by the winning collision resolver (the meteorite
        /// with the lower InstanceID) when it writes the post-collision velocity onto the
        /// other meteorite of the pair.
        /// </summary>
        public void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }


        public void Die()
        {
            // Order: split → notify → destroy. Children spawn before NotifyDestroyed so
            // subscribers see the OnMeteoriteSpawned (children) events ahead of this
            // meteorite's OnMeteoriteDestroyed. Small ignores the split call.
            _spawner.SpawnSplitChildren(_size, transform.position, _velocity);

            _spawner.NotifyDestroyed(_size, transform.position);

            Destroy(gameObject);
        }

        private void Update()
        {
            // 1. Straight-line motion along the current velocity vector.
            transform.position += (Vector3)_velocity * Time.deltaTime;

            // 2. Visual rotation around Z (2D game in xy plane). Sign of
            //    _rotationDegPerSecond selects CW vs CCW.
            transform.Rotate(0f, 0f, _rotationDegPerSecond * Time.deltaTime);

            // 3. Despawn once the meteorite has left the screen downward.
            if (transform.position.y < _despawnY)
            {
                Destroy(gameObject);
            }

            // Horný despawn: variant C bez clamp-u smie poslať meteorit hore; zachytíme ho
            // PRED bez-stenovou zónou (scenár 2 leak aj scenár 3 x-únik naraz).
            if (transform.position.y > _despawnYTop)
                Destroy(gameObject);
        }

        /// <summary>
        /// Meteorite↔meteorite collision handler. Both meteorites of a pair fire this
        /// handler in the same physics step; an InstanceID guard makes only one of them
        /// (the lower ID) compute the response and write the result onto both.
        ///
        /// Physics: fully elastic 2D collision. Velocity of each meteorite is decomposed
        /// onto the collision normal n = (B - A).normalized and the tangent. Tangential
        /// components survive unchanged; normal components are redistributed using the
        /// standard mass-weighted elastic-collision formula.
        ///
        /// Wall collisions are intentionally not handled here — Wall × Meteorite is
        /// disabled in the layer collision matrix and will be added in part 3 of the
        /// meteorites work.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            Meteorite otherMeteorite = other.GetComponent<Meteorite>();
            if (otherMeteorite == null)
            {
                // Not a meteorite — wall collisions are out of scope for this part.
                if (IsWall(other))
                    ResolveWallCollision(other);
                return;
            }

            // ID guard: each pair resolves only once, on the side with the lower InstanceID.
            if (GetInstanceID() > otherMeteorite.GetInstanceID())
            {
                return;
            }

            // Read both states before any write.
            Vector2 posA = transform.position;
            Vector2 posB = otherMeteorite.transform.position;
            Vector2 vA = _velocity;
            Vector2 vB = otherMeteorite.Velocity;
            float mA = _mass;
            float mB = otherMeteorite.Mass;

            // Collision normal for the circle-circle case: unit vector from A to B.
            Vector2 n = (posB - posA).normalized;

            // Decompose vA, vB into normal + tangential components.
            float aN = vA.x * n.x + vA.y * n.y;   // == Vector2.Dot(vA, n)
            float bN = vB.x * n.x + vB.y * n.y;

            Vector2 vA_v_n = aN * n;
            Vector2 vA_v_t = vA - vA_v_n;

            Vector2 vB_v_n = bN * n;
            Vector2 vB_v_t = vB - vB_v_n;

            // Mass-weighted elastic redistribution of the normal components;
            // tangential components survive unchanged.
            Vector2 vA_new = ((mA - mB) * vA_v_n + 2 * mB * vB_v_n) / (mA + mB) + vA_v_t;
            Vector2 vB_new = ((mB - mA) * vB_v_n + 2 * mA * vA_v_n) / (mA + mB) + vB_v_t;

            // Hook for Q3 (option B): if the prototype shows meteorites frequently
            // gaining a slight +y after collisions, clamp the y-component and renormalize
            // magnitude here. Currently disabled — option A (accept realistic physics).
            // See Otvorené otázky #6.

            _velocity = vA_new;
            otherMeteorite.SetVelocity(vB_new);
        }

        private bool IsWall(Collider2D col)
        {
            // _wallLayer je LayerMask z Inspectora; bitový test = "patrí vrstva col do masky?"
            return (_wallLayer.value & (1 << col.gameObject.layer)) != 0;
        }

        private void ResolveWallCollision(Collider2D wallCollider)
        {
            // Žiadny ID guard, žiadny zápis druhej strane: stena nemá handler,
            // meteorit je jediný resolver

            // --- TODO 1 (tvoje): kolízna normála z triggeru ---
            // Zavolaj _collider.Distance(wallCollider). Z výsledku (ColliderDistance2D)
            // vezmi .normal a urob z neho jednotkový Vector2 'n'
            // (defenzívne .normalized — caveat 2 zo Stop & Learn).
            Vector2 n = _collider.Distance(wallCollider).normal.normalized;

            // --- TODO 2 (tvoje): čistá reflexia v' = v - 2(v·n)n ---
            // Z _velocity a 'n' spočítaj odrazenú velocity (Vector2.Dot) a zapíš ju
            // PRIAMO do _velocity. NIE SetVelocity — to je na zápis DRUHÉMU meteoritu;
            // tu píšeš sebe. Žiadny clamp (variant C), vzorec je sign-invariantný v n.
            /* TODO 2 */
            _velocity = _velocity - 2f * Vector2.Dot(_velocity, n) * n;
        }
    }
}
