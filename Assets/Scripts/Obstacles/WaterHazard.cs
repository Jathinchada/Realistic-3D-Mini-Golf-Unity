using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Trigger component on rivers and ponds that catches balls in water hazards
    /// and resets them with audio feedback.
    /// </summary>
    public class WaterHazard : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null)
            {
                ball.TriggerWaterHazard();
            }
        }
    }
}
