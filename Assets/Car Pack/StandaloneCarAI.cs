using UnityEngine;

public class StandaloneCarAI : MonoBehaviour
{
    public Transform ball;
    public Transform myGoal;
    public Transform enemyGoal;
    public float acceleration = 100f;
    public float turnSpeed = 200f;
    public float stopDistance = 1.5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (ball == null || myGoal == null || enemyGoal == null) return;

        // Decide: Attack or Defend
        bool defending = IsBallCloserToMyGoal();

        Vector2 target;
        if (defending)
        {
            // Go between ball and own goal (block)
            target = (Vector2)ball.position + ((Vector2)myGoal.position - (Vector2)ball.position).normalized * 2f;
        }
        else
        {
            // Attack: aim for a point behind the ball (relative to enemy goal)
            Vector2 approachDir = ((Vector2)enemyGoal.position - (Vector2)ball.position).normalized;
            target = (Vector2)ball.position + approachDir * 1.5f;
        }

        MoveTowards(target);
    }

    bool IsBallCloserToMyGoal()
    {
        float distToMyGoal = Vector2.Distance(ball.position, myGoal.position);
        float distToEnemyGoal = Vector2.Distance(ball.position, enemyGoal.position);
        return distToMyGoal < distToEnemyGoal;
    }

    void MoveTowards(Vector2 target)
    {
        Vector2 toTarget = (target - (Vector2)transform.position);
        float distance = toTarget.magnitude;
        Vector2 dir = toTarget.normalized;

        float forwardDot = Vector2.Dot(transform.up, dir);
        float rightDot = Vector2.Dot(transform.right, dir);

        // Reduce steering when close to the ball to avoid circling
        float steeringFactor = (distance < 2f) ? 0.3f : 1f;

        // Steering
        float turn = -rightDot * steeringFactor;
        rb.AddTorque(turn * turnSpeed * Time.fixedDeltaTime);

        // Acceleration/Reverse
        float speed = (distance > stopDistance) ? forwardDot : 0f;
        rb.AddForce(transform.up * speed * acceleration * Time.fixedDeltaTime);
    }
} 