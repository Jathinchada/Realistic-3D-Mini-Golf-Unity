using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Trigger volume that alters the golf ball's gravity when inside.
    /// Supports Zero-G, custom directional gravity vectors, and inverted gravity.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AntiGravityZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField] private GravityMode mode = GravityMode.ZeroGravity;
        [SerializeField] private Vector3 customDirection = Vector3.up;
        [SerializeField] private float gravityMagnitude = 9.81f;
        [SerializeField] private int priority = 0;

        [Header("Visual Effects")]
        [SerializeField] private Color fieldColor = new Color(0.2f, 0.8f, 1f, 0.25f);
        [SerializeField] private bool pulseVisual = true;
        [SerializeField] private float pulseSpeed = 2.5f;

        private Collider zoneCollider;
        private Material fieldMaterial;
        private MeshRenderer meshRenderer;

        public GravityMode Mode => mode;
        public int Priority => priority;

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                fieldMaterial = meshRenderer.material;
                if (fieldMaterial != null)
                {
                    fieldMaterial.color = fieldColor;
                }
            }
        }

        private void Update()
        {
            if (pulseVisual && fieldMaterial != null)
            {
                float alpha = Mathf.PingPong(Time.time * pulseSpeed, 0.2f) + 0.15f;
                Color c = fieldColor;
                c.a = alpha;
                fieldMaterial.color = c;
            }
        }

        public Vector3 CalculateGravityVector()
        {
            switch (mode)
            {
                case GravityMode.ZeroGravity:
                    return Vector3.zero;

                case GravityMode.Inverted:
                    return new Vector3(0f, gravityMagnitude, 0f);

                case GravityMode.Directional:
                    return customDirection.normalized * gravityMagnitude;

                default:
                    return new Vector3(0f, -gravityMagnitude, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null && GravityManager.Instance != null)
            {
                GravityManager.Instance.EnterZone(this);
                if (ProceduralSoundManager.Instance != null)
                {
                    if (mode == GravityMode.ZeroGravity)
                        ProceduralSoundManager.Instance.PlayZeroGEnter();
                    else
                        ProceduralSoundManager.Instance.PlayGravityFlip();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null && GravityManager.Instance != null)
            {
                GravityManager.Instance.ExitZone(this);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = mode == GravityMode.ZeroGravity ? Color.cyan : (mode == GravityMode.Inverted ? Color.magenta : Color.yellow);
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }

            if (mode == GravityMode.Directional)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(transform.position, customDirection.normalized * 2f);
            }
        }
    }
}
