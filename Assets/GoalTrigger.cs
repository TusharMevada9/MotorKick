using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public enum GoalSide { Left, Right }
    public GoalSide goalSide;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
        {
            // Update score
            ScoreManager.Instance.GoalScored(goalSide);
            // Reset ball to center
            ball.ResetBall(new Vector2(6.9f, 41.3f));
        }
    }
} 