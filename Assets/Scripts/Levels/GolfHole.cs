using System.Collections;
using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Cup trigger: gently funnels the ball in, validates putt speed, sinks the ball,
    /// plays celebration, and reports the completed hole to GameManager.
    /// </summary>
    public class GolfHole : MonoBehaviour
    {
        [Header("Cup Settings (Reasonable Mini-Golf Cup)")]
        [SerializeField] private float maxSinkSpeed       = 8.5f;  // realistic mini-golf putt speed
        [SerializeField] private float funnelStrength     = 16f;   // gentle pull into sunken cup
        [SerializeField] private float sinkProximity      = 0.38f; // reasonable cup capture radius (proportional to ball)
        [SerializeField] private Transform cupCenter;              // set by CourseBuilder
        [SerializeField] private ParticleSystem celebrationParticles;

        private bool isCompleted = false;
        private bool isSinking   = false; // guard against double-trigger

        private void OnTriggerStay(Collider other)
        {
            if (isCompleted || isSinking) return;

            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball == null) return;

            Rigidbody rb = ball.BallRigidbody;
            if (rb == null) return;

            Vector3 center = cupCenter != null ? cupCenter.position : (transform.position + Vector3.down * 0.15f);
            Vector3 toCenter = center - ball.transform.position;
            // Downward funnel pull into the sunken hole
            toCenter.y -= 0.20f;
            rb.AddForce(toCenter.normalized * funnelStrength, ForceMode.Acceleration);

            float speed = rb.linearVelocity.magnitude;
            float horizontalDist = Vector2.Distance(
                new Vector2(center.x, center.z),
                new Vector2(ball.transform.position.x, ball.transform.position.z)
            );

            // Sink into the reasonable cup when centered and moving at putt speed
            if (horizontalDist < sinkProximity && speed < maxSinkSpeed)
            {
                isSinking = true;
                StartCoroutine(SinkSequence(ball));
            }
        }

        private IEnumerator SinkSequence(GolfBallController ball)
        {
            isCompleted = true;

            Rigidbody rb = ball.BallRigidbody;
            rb.isKinematic     = true;
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Smooth drop animation down into the sunken bottom of the cup
            Vector3 startPos  = ball.transform.position;
            Vector3 cupPos    = (cupCenter != null ? cupCenter.position : (transform.position + Vector3.down * 0.16f)) + Vector3.up * 0.08f;
            float   elapsed   = 0f;
            float   duration  = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                ball.transform.position = Vector3.Lerp(startPos, cupPos, t);
                yield return null;
            }

            ball.transform.position = cupPos;

            // Celebration effects
            if (celebrationParticles != null)
                celebrationParticles.Play();

            ProceduralSoundManager.Instance?.PlayHoleComplete();

            // Short pause so the player can see the ball sink
            yield return new WaitForSeconds(0.6f);

            GameManager.Instance?.CompleteCurrentHole();
        }

        public void ResetHole()
        {
            StopAllCoroutines();
            isCompleted = false;
            isSinking   = false;
        }
    }
}
