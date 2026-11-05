using UnityEngine;

namespace AntiGravityGolf
{
    /// <summary>
    /// Trigger zone covering the space void beneath and around the course.
    /// Respawns the ball back at the previous checkpoint if it falls off.
    /// </summary>
    public class OutOfBoundsZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            GolfBallController ball = other.GetComponent<GolfBallController>();
            if (ball != null)
            {
                ball.RespawnAtLastSafePosition();
            }
        }
    }
}
