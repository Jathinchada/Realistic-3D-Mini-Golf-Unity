using System.Collections;
using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Futuristic pinball-style bounce bumper that adds impulse and triggers bounce effects when struck.
    /// </summary>
    public class Bumper : MonoBehaviour
    {
        [Header("Bumper Settings")]
        [SerializeField] private float bounceForce = 12f;
        [SerializeField] private Color flashColor = new Color(0f, 1f, 0.8f); // Neon Cyan

        private Vector3 originalScale;
        private Material bumperMat;
        private Color originalColor;

        private void Awake()
        {
            originalScale = transform.localScale;
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                bumperMat = mr.material;
                originalColor = bumperMat.color;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            GolfBallController ball = collision.collider.GetComponent<GolfBallController>();
            if (ball != null)
            {
                // Normal direction away from bumper
                Vector3 bounceDir = (ball.transform.position - transform.position).normalized;
                if (bounceDir.sqrMagnitude < 0.01f) bounceDir = collision.contacts[0].normal;

                Rigidbody rb = ball.BallRigidbody;
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);
                }

                if (ProceduralSoundManager.Instance != null)
                {
                    ProceduralSoundManager.Instance.PlayBounce(1f);
                }

                StartCoroutine(PunchAnimation());
            }
        }

        private IEnumerator PunchAnimation()
        {
            transform.localScale = originalScale * 1.35f;
            if (bumperMat != null) bumperMat.color = flashColor;

            float elapsed = 0f;
            float duration = 0.25f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale * 1.35f, originalScale, t);
                if (bumperMat != null) bumperMat.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }

            transform.localScale = originalScale;
            if (bumperMat != null) bumperMat.color = originalColor;
        }
    }
}
