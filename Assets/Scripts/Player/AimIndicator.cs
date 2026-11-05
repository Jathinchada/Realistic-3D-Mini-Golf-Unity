using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Renders a dynamic aiming arrow and projected trajectory line from the golf ball.
    /// Visual cues scale in length and interpolate color based on shot power.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class AimIndicator : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private float baseLength = 1.0f;
        [SerializeField] private float maxLength = 5.0f;
        [SerializeField] private Color lowPowerColor = Color.cyan;
        [SerializeField] private Color midPowerColor = Color.yellow;
        [SerializeField] private Color highPowerColor = new Color(1f, 0.2f, 0.4f); // Neon Magenta-Red

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.12f;
            lineRenderer.endWidth = 0.02f;

            // Simple additive or unlit material
            Material mat = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material = mat;

            Hide();
        }

        public void Show(Vector3 origin, Vector3 direction, float powerRatio)
        {
            if (lineRenderer == null) return;
            lineRenderer.enabled = true;

            float length = Mathf.Lerp(baseLength, maxLength, powerRatio);
            Vector3 endpoint = origin + direction.normalized * length;

            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endpoint);

            Color currentColor = powerRatio < 0.5f 
                ? Color.Lerp(lowPowerColor, midPowerColor, powerRatio * 2f) 
                : Color.Lerp(midPowerColor, highPowerColor, (powerRatio - 0.5f) * 2f);

            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.2f);
        }

        public void Hide()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }
    }
}
