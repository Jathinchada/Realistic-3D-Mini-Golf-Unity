using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Rotates obstacles, barriers, or hazards around a designated axis at continuous or oscillating speed.
    /// </summary>
    public class RotatingObstacle : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float rotationSpeed = 60f; // Degrees per second
        [SerializeField] private bool oscillate = false;
        [SerializeField] private float maxOscillationAngle = 45f;
        [SerializeField] private float oscillationFrequency = 1.5f;

        private Quaternion initialRotation;

        private void Start()
        {
            initialRotation = transform.localRotation;
        }

        private void Update()
        {
            if (oscillate)
            {
                float angle = Mathf.Sin(Time.time * oscillationFrequency) * maxOscillationAngle;
                transform.localRotation = initialRotation * Quaternion.AngleAxis(angle, rotationAxis);
            }
            else
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
