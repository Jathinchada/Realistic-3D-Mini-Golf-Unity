using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Accelerates the golf ball forward in the boost pad's direction with an energizing impulse.
    /// </summary>
    public class BoostPad : MonoBehaviour
    {
        [Header("Boost Settings")]
        [SerializeField] private float boostForce = 18f;
        [SerializeField] private Vector3 boostDirection = Vector3.forward; // Relative to transform
        [SerializeField] private float cooldown = 0.5f;

        private float lastBoostTime = -99f;
        private Material padMat;

        private void Awake()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null) padMat = mr.material;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - lastBoostTime < cooldown) return;

            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null)
            {
                lastBoostTime = Time.time;
                Vector3 worldDir = transform.TransformDirection(boostDirection.normalized);

                Rigidbody rb = ball.BallRigidbody;
                if (rb != null)
                {
                    rb.linearVelocity = worldDir * boostForce;
                }

                if (ProceduralSoundManager.Instance != null)
                {
                    ProceduralSoundManager.Instance.PlayBoost();
                }
            }
        }
    }
}
