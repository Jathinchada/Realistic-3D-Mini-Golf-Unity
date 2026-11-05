using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Smooth orbital follow camera with dynamic gravity alignment and
    /// top-down Map Overview mode (press 'M' or 'O' to toggle bird's-eye course view).
    /// </summary>
    public class GolfCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance & Angles")]
        [SerializeField] private float defaultDistance = 5.8f;
        [SerializeField] private float minDistance = 2.5f;
        [SerializeField] private float maxDistance = 16f;
        [SerializeField] private float defaultPitch = 22f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 75f;

        [Header("Sensitivity & Smoothing")]
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private float positionFollowSpeed = 10f;
        [SerializeField] private float rotationSmoothSpeed = 8f;
        [SerializeField] private float upVectorAlignSpeed = 5f;

        [Header("Course Map Overview Mode")]
        [SerializeField] private Vector3 mapOverviewPosition = new Vector3(0f, 52f, -1f);
        [SerializeField] private Vector3 mapOverviewRotation = new Vector3(88f, 0f, 0f);
        [SerializeField] private bool isOverviewMode = false;

        private float currentYaw = 0f;
        private float currentPitch = 22f;
        private float currentDistance = 5.8f;
        private Vector3 currentUp = Vector3.up;

        public Transform Target { get => target; set => target = value; }
        public bool IsOverviewMode => isOverviewMode;
        public float CurrentYaw { get => currentYaw; set => currentYaw = value; }

        public void SetYaw(float yaw)
        {
            currentYaw = yaw;
        }

        private void Awake()
        {
            if (target == null)
            {
                GolfBallController b = FindObjectOfType<GolfBallController>();
                if (b != null) target = b.transform;
            }
        }

        private void Start()
        {
            if (target == null)
            {
                GolfBallController b = FindObjectOfType<GolfBallController>();
                if (b != null) target = b.transform;
            }
            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
                SnapToTarget();
            }
            currentPitch = defaultPitch;
            currentDistance = defaultDistance;
            currentUp = Vector3.up;
        }

        private void Update()
        {
            // Toggle course map overview mode via 'M' or 'O'
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.O))
            {
                ToggleMapOverview();
            }
        }

        private void LateUpdate()
        {
            if (isOverviewMode)
            {
                // Smoothly interpolate to bird's-eye view over whole 18-hole park
                transform.position = Vector3.Lerp(transform.position, mapOverviewPosition, Time.deltaTime * 6f);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(mapOverviewRotation), Time.deltaTime * 6f);
                return;
            }

            if (target == null) return;

            Vector3 targetUp = GravityManager.Instance != null ? GravityManager.Instance.EffectiveUpVector : Vector3.up;
            currentUp = Vector3.Slerp(currentUp, targetUp, Time.deltaTime * upVectorAlignSpeed);

            // Orbit controls: holding Right Mouse Button or Left Alt to orbit camera
            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftAlt))
            {
                currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity * 1.5f;
                currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 1.5f;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }
            else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
            {
                float spin = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
                currentYaw += spin * 75f * Time.deltaTime;
            }

            // Scroll zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentDistance = Mathf.Clamp(currentDistance - scroll * 4.5f, minDistance, maxDistance);
            }

            // Compute rotation relative to dynamic up-vector
            Quaternion upRotation = Quaternion.FromToRotation(Vector3.up, currentUp);
            Quaternion yawPitchRot = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Quaternion targetRotation = upRotation * yawPitchRot;

            // Compute camera position
            Vector3 offset = targetRotation * new Vector3(0f, 0f, -currentDistance);
            Vector3 desiredPosition = target.position + (currentUp * 0.5f) + offset;

            // Smooth position & rotation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionFollowSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position + currentUp * 0.3f - transform.position, currentUp), Time.deltaTime * rotationSmoothSpeed);
        }

        public void ToggleMapOverview()
        {
            isOverviewMode = !isOverviewMode;
            if (ProceduralSoundManager.Instance != null)
                ProceduralSoundManager.Instance.PlayUIClick();
        }

        public void SetOverviewMode(bool overview)
        {
            isOverviewMode = overview;
        }

        public void SnapToTarget()
        {
            isOverviewMode = false;
            if (target == null)
            {
                GolfBallController b = FindObjectOfType<GolfBallController>();
                if (b != null) target = b.transform;
                if (target == null) return;
            }
            Vector3 targetUp = GravityManager.Instance != null ? GravityManager.Instance.EffectiveUpVector : Vector3.up;
            currentUp = targetUp;
            Quaternion upRotation = Quaternion.FromToRotation(Vector3.up, currentUp);
            Quaternion yawPitchRot = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Quaternion targetRotation = upRotation * yawPitchRot;
            Vector3 offset = targetRotation * new Vector3(0f, 0f, -currentDistance);
            transform.position = target.position + (currentUp * 0.5f) + offset;
            transform.rotation = Quaternion.LookRotation(target.position + currentUp * 0.3f - transform.position, currentUp);
        }
    }
}
