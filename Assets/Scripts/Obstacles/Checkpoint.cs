using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Course checkpoint that records the ball's position and active gravity mode upon contact.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private Color activatedColor = new Color(0.2f, 1f, 0.4f); // Neon Green
        [SerializeField] private Color inactiveColor = new Color(0.2f, 0.5f, 1f); // Neon Blue

        private bool isActivated = false;
        private Material padMat;

        private void Awake()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                padMat = mr.material;
                SetColor(inactiveColor);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActivated) return;

            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null)
            {
                isActivated = true;
                Vector3 currentGrav = GravityManager.Instance != null ? GravityManager.Instance.CurrentGravity : Vector3.down * 9.81f;
                GravityMode mode = GravityManager.Instance != null ? GravityManager.Instance.CurrentMode : GravityMode.Normal;

                ball.SetCheckpoint(transform.position + Vector3.up * 0.2f, currentGrav, mode);
                SetColor(activatedColor);

                if (ProceduralSoundManager.Instance != null)
                {
                    ProceduralSoundManager.Instance.PlayCheckpoint();
                }
            }
        }

        public void ResetCheckpoint()
        {
            isActivated = false;
            SetColor(inactiveColor);
        }

        private void SetColor(Color c)
        {
            if (padMat != null)
            {
                padMat.color = c;
                if (padMat.HasProperty("_EmissionColor"))
                {
                    padMat.EnableKeyword("_EMISSION");
                    padMat.SetColor("_EmissionColor", c);
                }
            }
        }
    }
}
