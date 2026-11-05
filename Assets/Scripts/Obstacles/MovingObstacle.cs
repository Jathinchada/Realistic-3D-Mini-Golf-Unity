using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Translates platforms or obstacles back and forth between two local offsets or waypoints.
    /// Uses sinusoidal easing for natural smooth movement.
    /// </summary>
    public class MovingObstacle : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private Vector3 moveOffset = new Vector3(4f, 0f, 0f);
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private bool useLocalSpace = true;

        private Vector3 startPosition;
        private Vector3 endPosition;

        private void Start()
        {
            startPosition = useLocalSpace ? transform.localPosition : transform.position;
            endPosition = startPosition + moveOffset;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; // 0 to 1 smooth oscillation
            Vector3 targetPos = Vector3.Lerp(startPosition, endPosition, t);

            if (useLocalSpace)
            {
                transform.localPosition = targetPos;
            }
            else
            {
                transform.position = targetPos;
            }
        }
    }
}
