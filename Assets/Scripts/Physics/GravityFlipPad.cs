using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Interactive surface pad that dynamically flips or redirects gravity when struck by the ball.
    /// Includes visual emission flash and procedural audio feedback.
    /// </summary>
    public class GravityFlipPad : MonoBehaviour
    {
        [Header("Flip Direction")]
        [SerializeField] private Vector3 targetGravityDirection = Vector3.up; // Flipped towards ceiling
        [SerializeField] private float gravityMagnitude = 9.81f;
        [SerializeField] private float launchImpulse = 2.5f; // Small impulse to assist transitioning to opposite surface
        [SerializeField] private float cooldownTime = 1.0f;

        [Header("Visuals")]
        [SerializeField] private Color activeEmissionColor = new Color(0.9f, 0.2f, 1f); // Neon Magenta
        [SerializeField] private Color idleEmissionColor = new Color(0.3f, 0.05f, 0.4f);

        private float lastFlipTime = -99f;
        private MeshRenderer meshRenderer;
        private Material padMaterial;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                padMaterial = meshRenderer.material;
                SetPadGlow(idleEmissionColor);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - lastFlipTime < cooldownTime) return;

            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null)
            {
                TriggerFlip(ball);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - lastFlipTime < cooldownTime) return;

            GolfBallController ball = collision.collider.GetComponent<GolfBallController>();
            if (ball != null)
            {
                TriggerFlip(ball);
            }
        }

        private void TriggerFlip(GolfBallController ball)
        {
            lastFlipTime = Time.time;

            Vector3 newGravity = targetGravityDirection.normalized * gravityMagnitude;
            GravityMode mode = (targetGravityDirection == Vector3.up) ? GravityMode.Inverted : GravityMode.Directional;

            if (GravityManager.Instance != null)
            {
                GravityManager.Instance.SetGravity(newGravity, mode, instant: false);
            }

            // Apply slight assist impulse towards the new surface
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null && launchImpulse > 0f)
            {
                rb.AddForce(-targetGravityDirection.normalized * launchImpulse, ForceMode.Impulse);
            }

            if (ProceduralSoundManager.Instance != null)
            {
                ProceduralSoundManager.Instance.PlayGravityFlip();
            }

            // Flash visual
            if (padMaterial != null)
            {
                SetPadGlow(activeEmissionColor * 2f);
                CancelInvoke(nameof(ResetPadGlow));
                Invoke(nameof(ResetPadGlow), 0.35f);
            }
        }

        private void SetPadGlow(Color c)
        {
            if (padMaterial != null)
            {
                padMaterial.color = c;
                if (padMaterial.HasProperty("_EmissionColor"))
                {
                    padMaterial.EnableKeyword("_EMISSION");
                    padMaterial.SetColor("_EmissionColor", c);
                }
            }
        }

        private void ResetPadGlow()
        {
            SetPadGlow(idleEmissionColor);
        }
    }
}
