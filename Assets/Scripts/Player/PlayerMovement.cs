using UnityEngine;

namespace SpaceRunner.Player
{
    /// <summary>
    /// Drives ship movement and rotation from the mouse cursor position.
    ///
    /// Movement model: constant total speed v_max, decomposed via sin(angle) into
    /// horizontal motion. The ship has a fixed y; the vertical component never moves
    /// the ship, but the angle is exposed via CurrentAngleRadians so other systems
    /// (e.g. DistanceTracker via cos-projection) can derive their own values from
    /// the same source of truth.
    ///
    /// Design rationale: 21.01.01 Koncept.md, section "Pohybový model lode".
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement parameters")]
        [Tooltip("Maximum total ship speed (world units / sec). Horizontal speed = v_max * sin(angle from vertical).")]
        [SerializeField] private float _maxSpeed = 5f;

        /// <summary>
        /// Current ship rotation angle from vertical, in radians.
        /// Range: -π/2 (90° left) to +π/2 (90° right). 0 = straight up.
        /// Pull-pattern surface for other systems (DistanceTracker, later Weapons).
        /// </summary>
        public float CurrentAngleRadians { get; private set; }

        /// <summary>Maximum total speed — single source of truth for downstream consumers (e.g. DistanceTracker uses it for cos-projection of progress).</summary>
        public float MaxSpeed => _maxSpeed;

        // Cached at Awake; Camera.main does a tag-based lookup each call,
        // which is too slow to repeat every frame.
        private Camera _mainCamera;

        /// <summary>Caches Camera.main once at startup (Camera.main is a tag lookup, not free).</summary>
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            // 1. Cursor in world coordinates.
            // ScreenToWorldPoint needs a z-distance from the camera.
            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = -_mainCamera.transform.position.z;
            Vector3 cursorWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);

            // 2. Clamp cursor below the ship.
            // Design rule: if the cursor sits below the ship, treat it as being on the
            // ship's y-line for the angle computation. This keeps |angle| ≤ 90°.
            if (cursorWorld.y < transform.position.y)
            {
                cursorWorld.y = transform.position.y;
            }

            // 3. Vector from ship to cursor.
            Vector2 toCursor = (Vector2)(cursorWorld - transform.position);

            // 3b. Dead zone around the ship.
            // If the cursor is right at the ship, Atan2(0, 0) is undefined; just below
            // it (after the y-clamp) Atan2 would oscillate between ±π/2 frame to frame,
            // producing visible bang-bang flicker. In the dead zone we simply hold state.
            const float DEAD_ZONE_RADIUS = 0.3f;
            if (toCursor.sqrMagnitude < DEAD_ZONE_RADIUS * DEAD_ZONE_RADIUS)
            {
                return;
            }

            // 4. Angle from vertical, in radians.
            // Mathf.Atan2(x, y) with swapped argument order returns the angle from +Y.
            // Positive = cursor on the right, negative = cursor on the left.
            CurrentAngleRadians = Mathf.Atan2(toCursor.x, toCursor.y);

            // 5. Apply rotation.
            // Unity 2D rotation around Z is counter-clockwise (positive = visually left).
            // Our angle is clockwise-from-vertical, hence the minus sign.
            float angleDegrees = -CurrentAngleRadians * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

            // 6. Apply horizontal motion.
            // horizontal_velocity = v_max * sin(angle). Time.deltaTime → frame-rate independent.
            float deltaX = _maxSpeed * Mathf.Sin(CurrentAngleRadians) * Time.deltaTime;
            Vector3 pos = transform.position;
            pos.x += deltaX;
            transform.position = pos;
        }
    }
}
