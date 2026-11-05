using System;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_6000_0_OR_NEWER
using BallPhysicMaterial = UnityEngine.PhysicsMaterial;
using BallPhysicCombine  = UnityEngine.PhysicsMaterialCombine;
#else
using BallPhysicMaterial = UnityEngine.PhysicMaterial;
using BallPhysicCombine  = UnityEngine.PhysicMaterialCombine;
#endif

namespace AntiGravityGolf
{
    /// <summary>
    /// Rock-solid, highly playable golf ball controller.
    ///
    /// CONTROLS:
    ///   - Hold Left Mouse Button and drag backward to aim and charge power.
    ///   - OR: Aim with Mouse / A / D / Arrow Keys and HOLD SPACEBAR to charge power, RELEASE SPACEBAR to shoot!
    ///   - Right-click while aiming/charging to cancel.
    ///   - R to restart ball / hole.
    ///   - Esc or P to pause.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class GolfBallController : MonoBehaviour
    {
        [Header("Shot Tuning")]
        [SerializeField] private float minShotSpeed     = 2.4f;   // solid gentle tap
        [SerializeField] private float maxShotSpeed     = 19.5f;  // weighted full drive
        [SerializeField] private float powerCurveExp    = 1.25f;  // non-linear feel
        [SerializeField] private float dragPixelsForMax = 220f;   // drag distance for full power
        [SerializeField] private float spaceChargeRate  = 0.95f;  // charge power per second with spacebar

        [Header("Stopping Detection")]
        [SerializeField] private float stopSpeedThreshold = 0.08f;
        [SerializeField] private float stopDelaySeconds   = 0.20f;
        [SerializeField] private float safetyTimeout      = 14f;

        [Header("Ball Physics")]
        [SerializeField] private float ballMass           = 2.5f;   // weighted, solid physical golf ball
        [SerializeField] private float ballLinearDamping  = 0.55f;  // smooth grounded rolling resistance
        [SerializeField] private float ballAngularDamping = 0.45f;

        [Header("Visuals")]
        [SerializeField] private AimIndicator aimIndicator;
        [SerializeField] private TrailRenderer ballTrail;

        // State
        private Rigidbody        rb;
        private SphereCollider   col;
        private bool             isStopped       = true;
        private bool             isAiming        = false;
        private float            powerRatio      = 0f;
        private Vector3          aimDir          = Vector3.forward;
        private float            stopTimer       = 0f;
        private float            motionTimer     = 0f;
        private Vector3          dragStartSS;
        private bool             mouseDragging   = false;
        private bool             spaceCharging   = false;
        private float            spaceChargeDir  = 1f;
        private Vector3          lastSafePos;
        private Vector3          lastShotPos;
        private float            spawnGracePeriod = 0f;

        public bool    IsStopped           => isStopped;
        public bool    IsAiming            => isAiming;
        public float   CurrentPowerRatio   => powerRatio;
        public Vector3 CurrentAimDirection => aimDir;
        public Rigidbody BallRigidbody     => rb != null ? rb : (rb = GetComponent<Rigidbody>());

        public event Action OnShotTaken;
        public event Action OnBallStopped;
        public event Action OnRespawned;

        private void Awake()
        {
            rb  = GetComponent<Rigidbody>();
            col = GetComponent<SphereCollider>();

            rb.mass                   = ballMass;
            rb.linearDamping          = ballLinearDamping;
            rb.angularDamping         = ballAngularDamping;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation          = RigidbodyInterpolation.Interpolate;
            rb.useGravity             = true;

            var mat = new BallPhysicMaterial("GolfBall_PhysMat")
            {
                dynamicFriction = 0.22f,
                staticFriction  = 0.28f,
                bounciness      = 0.08f,
                frictionCombine = BallPhysicCombine.Average,
                bounceCombine   = BallPhysicCombine.Minimum
            };
            col.material = mat;

            lastSafePos = transform.position;
            lastShotPos = transform.position;
        }

        private void Start()
        {
            if (aimIndicator == null) aimIndicator = GetComponentInChildren<AimIndicator>();
            if (ballTrail == null)    ballTrail    = GetComponentInChildren<TrailRenderer>();
            if (ballTrail != null)    ballTrail.emitting = false;
        }

        private void Update()
        {
            if (spawnGracePeriod > 0f)
            {
                spawnGracePeriod -= Time.deltaTime;
                if (spawnGracePeriod < 0f) spawnGracePeriod = 0f;
            }

            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.IsPaused) return;
                if (GameManager.Instance.CurrentState != GameState.Playing)
                {
                    CancelAim();
                    return;
                }
            }

            // Auto-respawn safety if ball flies out of course bounds or falls
            if (transform.position.y < -12f || Mathf.Abs(transform.position.x) > 50f || Mathf.Abs(transform.position.z) > 40f)
            {
                RespawnAtLastSafePosition();
                return;
            }

            if (!isStopped) return;

            HandleAimAndInput();
        }

        private void FixedUpdate()
        {
            if (GravityManager.Instance != null)
            {
                rb.useGravity = false;
                GravityManager.Instance.ApplyGravityTo(rb);
            }
            else
            {
                rb.useGravity = true;
            }

            UpdateStopping();

            if (rb != null)
            {
                ProceduralSoundManager.Instance?.UpdateBallRoll(rb.linearVelocity.magnitude);
            }
        }

        private void HandleAimAndInput()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // 1. Right-click cancels aim
            if (Input.GetMouseButtonDown(1))
            {
                CancelAim();
                return;
            }

            // 2. Keyboard Aim Adjustments (A/D or Left/Right Arrow Keys)
            float hInput = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))  hInput -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) hInput += 1f;
            if (Mathf.Abs(hInput) > 0.01f)
            {
                aimDir = Quaternion.Euler(0f, hInput * 90f * Time.deltaTime, 0f) * aimDir;
                aimDir.y = 0f;
                aimDir.Normalize();
            }

            // 3. Spacebar Power Charging Mode
            if (Input.GetKeyDown(KeyCode.Space) && spawnGracePeriod <= 0f)
            {
                spaceCharging = true;
                isAiming = true;
                powerRatio = 0.05f;
                spaceChargeDir = 1f;

                // If aim direction isn't set, default forward based on camera or current yaw
                if (aimDir.sqrMagnitude < 0.1f)
                {
                    aimDir = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                }
            }

            if (spaceCharging && Input.GetKey(KeyCode.Space))
            {
                powerRatio += spaceChargeDir * spaceChargeRate * Time.deltaTime;
                if (powerRatio >= 1.0f)
                {
                    powerRatio = 1.0f;
                    spaceChargeDir = -1f;
                }
                else if (powerRatio <= 0.08f)
                {
                    powerRatio = 0.08f;
                    spaceChargeDir = 1f;
                }

                if (aimIndicator != null)
                    aimIndicator.Show(transform.position, aimDir, powerRatio);

                return;
            }

            if (spaceCharging && Input.GetKeyUp(KeyCode.Space))
            {
                spaceCharging = false;
                ExecuteShot();
                return;
            }

            // 4. Mouse Slingshot / Drag Mode
            if (Input.GetMouseButtonDown(0))
            {
                if (spawnGracePeriod > 0f) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

                mouseDragging = true;
                dragStartSS = Input.mousePosition;
                isAiming = true;
                powerRatio = 0.05f;
            }

            if (mouseDragging && Input.GetMouseButton(0))
            {
                Vector3 curSS = Input.mousePosition;
                Vector3 dragDelta = dragStartSS - curSS; // pull back to hit forward

                float dragDist = dragDelta.magnitude;
                powerRatio = Mathf.Clamp01(dragDist / dragPixelsForMax);

                if (dragDist > 10f)
                {
                    // Compute world aim direction relative to camera facing
                    Vector3 camFwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                    Vector3 camRgt = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
                    Vector3 worldDelta = (camRgt * dragDelta.x + camFwd * dragDelta.y).normalized;
                    if (worldDelta.sqrMagnitude > 0.01f)
                    {
                        aimDir = worldDelta;
                    }
                }
                else
                {
                    // If subtle drag, align aim in front of camera
                    aimDir = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                }

                if (aimIndicator != null)
                    aimIndicator.Show(transform.position, aimDir, powerRatio);
            }

            if (mouseDragging && Input.GetMouseButtonUp(0))
            {
                mouseDragging = false;
                ExecuteShot();
            }
        }

        private void ExecuteShot()
        {
            spawnGracePeriod = 0f;
            isAiming = false;
            mouseDragging = false;
            spaceCharging = false;
            if (aimIndicator != null) aimIndicator.Hide();

            if (powerRatio < 0.05f) powerRatio = 0.05f;

            lastShotPos = transform.position;
            lastSafePos = transform.position;

            float curved = Mathf.Pow(powerRatio, powerCurveExp);
            float speed  = Mathf.Lerp(minShotSpeed, maxShotSpeed, curved);

            Vector3 dir = aimDir;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
            {
                Camera c = Camera.main;
                if (c != null) dir = Vector3.ProjectOnPlane(c.transform.forward, Vector3.up);
            }
            if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
            dir.Normalize();

            rb.isKinematic = false;
            rb.linearVelocity = dir * speed;
            Vector3 spinAxis = Vector3.Cross(Vector3.up, dir);
            rb.angularVelocity = spinAxis * (speed / 0.19f);

            isStopped = false;
            stopTimer = 0f;
            motionTimer = 0f;

            if (ballTrail != null) { ballTrail.Clear(); ballTrail.emitting = true; }

            ProceduralSoundManager.Instance?.PlayPutt(powerRatio);
            GameManager.Instance?.RegisterStroke();
            OnShotTaken?.Invoke();
        }

        private void UpdateStopping()
        {
            if (isStopped) return;

            motionTimer += Time.fixedDeltaTime;
            float spd = rb.linearVelocity.magnitude;

            if (spd < stopSpeedThreshold)
            {
                stopTimer += Time.fixedDeltaTime;
                if (stopTimer >= stopDelaySeconds) StopClean();
            }
            else
            {
                stopTimer = 0f;
            }

            if (motionTimer > safetyTimeout && spd < 1.2f) StopClean();
        }

        private void StopClean()
        {
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            isStopped   = true;
            stopTimer   = 0f;
            motionTimer = 0f;
            lastSafePos = transform.position;

            if (ballTrail != null) ballTrail.emitting = false;
            OnBallStopped?.Invoke();
        }

        private void OnCollisionEnter(Collision col)
        {
            float impact = col.relativeVelocity.magnitude;
            if (impact > 0.4f)
                ProceduralSoundManager.Instance?.PlayBounce(Mathf.Clamp01(impact / 8f));
        }

        public void CancelAim()
        {
            isAiming = false;
            mouseDragging = false;
            spaceCharging = false;
            powerRatio = 0f;
            if (aimIndicator != null) aimIndicator.Hide();
        }

        public void SetCheckpoint(Vector3 position, Vector3 gravity, GravityMode mode)
        {
            lastSafePos = position;
        }

        public void TriggerWaterHazard()
        {
            ProceduralSoundManager.Instance?.PlayWaterSplash();
            GameManager.Instance?.RegisterPenalty("WATER HAZARD! +1 PENALTY STROKE");
            RespawnAtLastSafePosition();
        }

        public void SetAimDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                aimDir = direction.normalized;
            }
        }

        public void RespawnAtLastSafePosition()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            rb.isKinematic     = false;
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 targetPos  = lastSafePos + Vector3.up * 0.19f;
            transform.position = targetPos;
            rb.position        = targetPos;
            Physics.SyncTransforms();
            rb.WakeUp();

            isStopped        = true;
            stopTimer        = 0f;
            motionTimer      = 0f;
            spawnGracePeriod = 0.20f;
            CancelAim();

            if (ballTrail != null) { ballTrail.Clear(); ballTrail.emitting = false; }
            OnRespawned?.Invoke();
        }

        public void TeleportTo(Vector3 position)
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            rb.isKinematic     = false;
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = position;
            rb.position        = position;
            Physics.SyncTransforms();
            rb.WakeUp();

            lastShotPos      = position;
            lastSafePos      = position;
            isStopped        = true;
            stopTimer        = 0f;
            motionTimer      = 0f;
            spawnGracePeriod = 0.20f;
            CancelAim();

            if (ballTrail != null) { ballTrail.Clear(); ballTrail.emitting = false; }
        }
    }
}
