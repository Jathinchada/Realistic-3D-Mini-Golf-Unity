using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Animates gentle surface wave oscillation and shimmering ripple movement
    /// on mini-golf ponds for a peaceful, living outdoor park atmosphere.
    /// </summary>
    public class WaterSurfaceAnimation : MonoBehaviour
    {
        [SerializeField] private float waveSpeed = 0.55f;
        [SerializeField] private float waveAmount = 0.015f;
        [SerializeField] private float textureScrollSpeed = 0.04f;

        private Vector3 baseLocalPos;
        private Renderer meshRenderer;
        private Material waterMat;

        private void Start()
        {
            baseLocalPos = transform.localPosition;
            meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                waterMat = meshRenderer.material;
            }
        }

        private void Update()
        {
            float t = Time.time * waveSpeed;
            // Subtle vertical wave breathing
            transform.localPosition = baseLocalPos + new Vector3(0f, Mathf.Sin(t * 2.2f) * waveAmount, 0f);

            // Subtle UV offset shimmer if texture or normal map is present
            if (waterMat != null && waterMat.HasProperty("_MainTex"))
            {
                float scroll = Time.time * textureScrollSpeed;
                waterMat.mainTextureOffset = new Vector2(scroll * 0.5f, Mathf.Sin(scroll) * 0.15f);
            }
        }
    }
}
