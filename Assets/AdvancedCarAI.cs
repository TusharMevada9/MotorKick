using UnityEngine;

public class AdvancedCarAI : MonoBehaviour
{
    public Transform ball;
    public Transform enemyGoal;
    public Transform ownGoal;

    private Rigidbody2D rb;
    private Rigidbody2D ballRb;

    public float minAcceleration = 80f;
    public float maxAcceleration = 150f;
    public float turnSpeed = 100f;

    private float _acceleration;

    private enum AIState
    {
        Attack,
        Defend,
        Retreat,
    }

    private AIState state;

    private float cooldownTimer = 0f;

    [Header("Overcommitment Settings")]
    public float cooldownDuration = 1f;
    public float overcommitDistance = 1f;

    [Header("Goal Line Defense Settings")]
    public float goalLineDefenseDistance = 1.2f; // Distance from own goal to trigger goal line defense

    private float fieldBoundsX = 8f;
    private float fieldBoundsY = 4.5f;

    public int myScore = 0;
    public int enemyScore = 0;

    public GameObject playerCar;
    public GameObject aiCar;

    private Vector2 initialPlayerPos;
    private Vector2 initialAIPos;
    private Vector2 initialBallPos;

    public Vector3 playerResetPosition = new Vector3(-2.07f, -10.2f, 0f);
    public Vector3 aiResetPosition = new Vector3(6.09f, 80.8f, 0f);

    private Vector2 targetPos; // Moved to class level for Gizmo visualization
    private Vector2 approachPoint; // For visualization

    public float shotAlignmentThreshold = 20f; // Degrees, for shot alignment
    public float defensiveZoneThreshold = 2.5f; // Distance from own goal to trigger defense

    [Range(0f, 1f)]
    public float aggressionLevel = 0.5f; // 0 = passive, 1 = max aggression (auto-set by score difference)

    private CarBehavior carBehavior;
    private bool aiCanStartBoost = true;

    // Returns the ideal strategic position based on current state
    private Vector2 GetStrategicTargetPosition(
        Vector2 carPos,
        Vector2 ballPos,
        Vector2 enemyPos,
        Vector2 ownPos,
        Vector2 carToBall,
        Vector2 ballToEnemyGoal,
        Vector2 ballToOwnGoal
    )
    {
        switch (state)
        {
            case AIState.Defend:
                // If ball is very close to own goal, go directly to goal line for emergency defense
                float ballDistToOwnGoal = Vector2.Distance(ballPos, ownPos);
                if (ballDistToOwnGoal < goalLineDefenseDistance)
                {
                    // Position just in front of own goal (toward field center)
                    return ownPos + (ballPos - ownPos).normalized * 0.5f;
                }
                // Otherwise, position between ball and own goal
                return ballPos + ballToOwnGoal * 1.5f;
            case AIState.Attack:
                float angleToEnemyGoal = Vector2.Angle(carToBall, ballToEnemyGoal);
                // Calculate approach point behind the ball, aligned with enemy goal
                approachPoint = ballPos - ballToEnemyGoal * 1.0f;
                if (angleToEnemyGoal > 20f)
                {
                    // Not well-aligned, go to approach point
                    return approachPoint;
                }
                else
                {
                    // Well-aligned, go for the ball
                    return ballPos;
                }
            case AIState.Retreat:
                // Move away from the ball
                return carPos - carToBall * 2f;
            default:
                return ballPos;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody2D>();

        _acceleration = Random.Range(minAcceleration, maxAcceleration);
        initialPlayerPos = playerCar.transform.position;
        initialAIPos = aiCar.transform.position;
        initialBallPos = ball.position;

        carBehavior = GetComponent<CarBehavior>();
    }

    void Update()
    {
        if (ball == null || enemyGoal == null || ownGoal == null)
            return;
        if (ballRb == null)
            ballRb = ball.GetComponent<Rigidbody2D>();

        // Convert all positions to Vector2 to ensure correct math
        Vector2 carPos = rb.position;
        Vector2 ballPos = ballRb.position;
        Vector2 enemyPos = (Vector2)enemyGoal.position;
        Vector2 ownPos = (Vector2)ownGoal.position;

        float distToBall = Vector2.Distance(carPos, ballPos);
        float ballDistToOwnGoal = Vector2.Distance(ballPos, ownPos);
        float ballDistToEnemyGoal = Vector2.Distance(ballPos, enemyPos);
        bool ballMovingToOwnGoal = Vector2.Dot(ballRb.linearVelocity, ownPos - ballPos) > 0;

        // Declare winning/losing once for use throughout Update
        bool winning = myScore > enemyScore;
        bool losing = myScore < enemyScore;

        // Determine current state (improved logic)
        if (cooldownTimer > 0f)
        {
            state = AIState.Retreat;
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            bool ballInOwnHalf = (ballPos - ownPos).magnitude < (ballPos - enemyPos).magnitude;

            // If ball is very close to own goal or moving toward own goal, defend
            if (
                (ballMovingToOwnGoal && ballDistToOwnGoal < 4f)
                || ballDistToOwnGoal < defensiveZoneThreshold
            )
            {
                state = AIState.Defend;
            }
            // If winning, prefer defense when ball is in own half
            else if (winning && ballInOwnHalf)
            {
                state = AIState.Defend;
            }
            // If losing, prefer attack unless ball is dangerously close to own goal
            else if (losing && ballDistToOwnGoal > defensiveZoneThreshold)
            {
                state = AIState.Attack;
            }
            // Default: attack
            else
            {
                state = AIState.Attack;
            }
        }

        // Adaptive Aggression
        bool ballFarFromOwnGoal = ballDistToOwnGoal > 5f;
        // Set aggressionLevel based on score difference
        int scoreDiff = myScore - enemyScore;
        if (scoreDiff < 0)
        {
            // Losing: more aggressive, scale up with how much we're losing
            aggressionLevel = Mathf.Clamp01(0.5f + Mathf.Abs(scoreDiff) * 0.2f);
        }
        else if (scoreDiff > 0)
        {
            // Winning: less aggressive, scale down with how much we're winning
            aggressionLevel = Mathf.Clamp01(0.5f - scoreDiff * 0.2f);
        }
        else
        {
            // Tied
            aggressionLevel = 0.5f;
        }
        // Optionally, increase aggression if ball is far from own goal
        if (ballFarFromOwnGoal)
            aggressionLevel = Mathf.Clamp01(aggressionLevel + 0.2f);

        // Interpolate acceleration based on aggressionLevel
        _acceleration = Mathf.Lerp(minAcceleration, maxAcceleration, aggressionLevel);

        // Directions (all Vector2)
        Vector2 ballToEnemyGoal = (enemyPos - ballPos).normalized;
        Vector2 ballToOwnGoal = (ownPos - ballPos).normalized;
        Vector2 carToBall = (ballPos - carPos).normalized;

        // Strategic positioning
        targetPos = GetStrategicTargetPosition(
            carPos,
            ballPos,
            enemyPos,
            ownPos,
            carToBall,
            ballToEnemyGoal,
            ballToOwnGoal
        );

        // Overcommitment check: if ball is near wall and AI is too close, trigger retreat cooldown
        bool ballNearWall =
            Mathf.Abs(ballPos.x) > fieldBoundsX - 0.5f
            || Mathf.Abs(ballPos.y) > fieldBoundsY - 0.5f;
        if (ballNearWall && distToBall < overcommitDistance)
        {
            state = AIState.Retreat;
            cooldownTimer = cooldownDuration;
        }

        // Movement execution
        Vector2 toTarget = targetPos - carPos;
        float distToTarget = toTarget.magnitude;
        if (distToTarget > 0.01f)
        {
            Vector2 toTargetDir = toTarget / distToTarget;

            // Rotate smoothly
            float angle = Mathf.Atan2(toTargetDir.y, toTargetDir.x) * Mathf.Rad2Deg - 90f;
            float newAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, turnSpeed * Time.deltaTime);
            rb.MoveRotation(newAngle);

            // Smooth forward movement
            if (distToTarget > 0.15f)
            {
                Vector2 desiredVelocity = toTargetDir * _acceleration * Time.deltaTime;
                rb.linearVelocity = Vector2.Lerp(
                    rb.linearVelocity,
                    rb.linearVelocity + desiredVelocity,
                    0.2f
                );
            }
        }

        // Push logic (only in Attack state)
        if (state == AIState.Attack && distToBall < 0.6f)
        {
            float pushAlignment = Vector2.Angle(carToBall, ballToEnemyGoal);
            // Visualize alignment for debugging
            Debug.DrawLine(
                rb.position,
                rb.position + (Vector2)ballToEnemyGoal * 2f,
                pushAlignment < shotAlignmentThreshold ? Color.green : Color.red
            );
            if (pushAlignment < shotAlignmentThreshold)
            {
                // Ensure you're pushing toward enemy goal, not own goal
                float dot = Vector2.Dot(ballToEnemyGoal, ballToOwnGoal);
                if (dot < 0f)
                {
                    // Well-aligned shot: apply force toward enemy goal
                    rb.AddForce(ballToEnemyGoal * _acceleration * Time.deltaTime);
                }
            }
        }

        // AI Burst Boost Logic
        if (carBehavior != null)
        {
            bool wantBoost = (state == AIState.Attack && distToBall < 1.2f);
            if (
                wantBoost
                && aiCanStartBoost
                && Mathf.Approximately(carBehavior.currentBoost, carBehavior.maxBoost)
            )
            {
                carBehavior.boosting = true;
            }
            else if (wantBoost && carBehavior.boosting && carBehavior.currentBoost > 0f)
            {
                carBehavior.boosting = true;
            }
            else
            {
                carBehavior.boosting = false;
            }
            // AI can only start boost if bar is full and not currently boosting
            if (!carBehavior.boosting && carBehavior.currentBoost < carBehavior.maxBoost)
            {
                aiCanStartBoost = false;
            }
            if (
                !carBehavior.boosting
                && Mathf.Approximately(carBehavior.currentBoost, carBehavior.maxBoost)
            )
            {
                aiCanStartBoost = true;
            }
        }
    }

    public void ResetPositions()
    {
        playerCar.transform.position = playerResetPosition;
        var playerRb = playerCar.GetComponent<Rigidbody2D>();
        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;

        aiCar.transform.position = aiResetPosition;
        var aiRb = aiCar.GetComponent<Rigidbody2D>();
        aiRb.linearVelocity = Vector2.zero;
        aiRb.angularVelocity = 0f;

        ball.position = initialBallPos;
        var ballRb = ball.GetComponent<Rigidbody2D>();
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
    }

    void OnDrawGizmos()
    {
        if (ball != null && enemyGoal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ball.position, enemyGoal.position);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, ball.position);
        }
        // Visualize AI's current target position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPos, 0.2f);
        // Visualize approach point (only in Attack)
        if (state == AIState.Attack)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(approachPoint, 0.15f);
            Gizmos.DrawLine(ball.position, approachPoint);
        }
        // Visualize overcommitment zone around ball
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // Orange, semi-transparent
        Gizmos.DrawWireSphere(ball.position, overcommitDistance);
    }
}
