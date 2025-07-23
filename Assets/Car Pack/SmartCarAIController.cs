using UnityEngine;

public class SmartCarAIController : MonoBehaviour
{
    public Transform ball;
    public Transform myGoal;      // Assign your own goal's Transform
    public Transform enemyGoal;   // Assign the opponent's goal's Transform
    public float stopDistance = 1.5f;

    private CarBehavior carBehavior;

    void Start()
    {
        carBehavior = GetComponent<CarBehavior>();
    }

    void Update()
    {
        if (ball == null || carBehavior == null || myGoal == null || enemyGoal == null) return;

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
            // Attack: aim for the ball, but approach from the side facing the enemy goal
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

        // Steering
        if (rightDot < -0.1f)
            carBehavior.Left();
        else if (rightDot > 0.1f)
            carBehavior.Right();
        else
            carBehavior.Straight();

        // Acceleration/Reverse
        if (distance > stopDistance)
        {
            if (forwardDot > 0.1f)
                carBehavior.gas_pedal = 1f; // Forward
            else if (forwardDot < -0.1f)
                carBehavior.gas_pedal = -1f; // Reverse
            else
                carBehavior.gas_pedal = 0f;
        }
        else
        {
            carBehavior.gas_pedal = 0f;
        }
    }
} 