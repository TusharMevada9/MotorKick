using UnityEngine;

public class StandaloneCarAI : MonoBehaviour
{
    public Transform ball;
    public Transform enemyGoal;

    public GameObject CarFront;
    private float _acceleration;
    public float turnSpeed = 100f; // વધુ soft turn
    public float minAcceleration = 80f;
    public float maxAcceleration = 150f;
    private Rigidbody2D rb;

    private enum AIState { Pushing, Reversing, Charging }
    private AIState state = AIState.Pushing;
    private float stateTimer = 0f;
    private float stuckCheckTimer = 0f;
    private Rigidbody2D ballRb;
    public float sideApproachOffset = 1.2f; // Inspector: how far to approach from side
    private bool approachingFromSide = false;
    private Vector2 sideApproachTarget;

    public bool easyMode;
    public bool hardMode;

    private bool easyCooldownTimerActive = false;
    private float easyCooldownTimer = 0f;
    private Vector2 easyTarget = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody2D>();
        // Always assign a random acceleration
        _acceleration = Random.Range(minAcceleration, maxAcceleration);
    }

    void Update()
    {
        if (ball == null || CarFront == null || enemyGoal == null) return;
        if (ballRb == null) ballRb = ball.GetComponent<Rigidbody2D>();

        if (hardMode)
        {
            float stopDistanceToGoal = 0.7f;
            float ballToGoalDist = Vector2.Distance(ball.position, enemyGoal.position);
            if (ballToGoalDist < stopDistanceToGoal)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                return;
            }

            // --- Stuck Detection ---
            stuckCheckTimer += Time.deltaTime;
            if (state == AIState.Pushing && stuckCheckTimer > 0.5f)
            {
                float carSpeed = rb.linearVelocity.magnitude;
                float ballSpeed = ballRb != null ? ballRb.linearVelocity.magnitude : 0f;
                if (carSpeed < 0.5f && ballSpeed < 0.2f)
                {
                    state = AIState.Reversing;
                    stateTimer = 0.5f; // reverse for 0.5s
                }
                stuckCheckTimer = 0f;
            }

            // --- Always rotate car to face the ball ---
            Vector2 toBallDir = (ball.position - transform.position).normalized;
            float angle = Mathf.Atan2(toBallDir.y, toBallDir.x) * Mathf.Rad2Deg - 90f; // -90 to align with up
            float currentAngle = rb.rotation;
            // Smooth rotation for hard mode
            float smoothTurnSpeed = turnSpeed * 0.8f;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, angle, smoothTurnSpeed * Time.deltaTime);
            rb.MoveRotation(newAngle);

            // --- Side approach logic ---
            float carToBallDist = Vector2.Distance(transform.position, ball.position);
            if (carToBallDist > 1.5f)
            {
                // Pick left or right side randomly every time car is far from ball
                Vector2 ballToGoal = (enemyGoal.position - ball.position).normalized;
                Vector2 side = Vector2.Perpendicular(ballToGoal) * (Random.value < 0.5f ? 1f : -1f);
                sideApproachTarget = (Vector2)ball.position + side * sideApproachOffset;
                approachingFromSide = true;
            }
            if (carToBallDist < 0.5f)
            {
                approachingFromSide = false;
            }
            // If approaching from side, go to that target first
            if (approachingFromSide)
            {
                float distToSideTarget = Vector2.Distance(transform.position, sideApproachTarget);
                if (distToSideTarget > 0.2f)
                {
                    Vector2 dir = (sideApproachTarget - (Vector2)transform.position).normalized;
                    rb.AddForce(dir * _acceleration * Time.deltaTime);
                    return;
                }
                else
                {
                    approachingFromSide = false;
                }
            }

            // --- State Machine ---
            switch (state)
            {
                case AIState.Pushing:
                    {
                        // Usual logic: get behind ball, push toward goal
                        Vector2 ballToGoal = (enemyGoal.position - ball.position).normalized;
                        float behindBallDist = 0.7f;
                        Vector2 targetPos = (Vector2)ball.position - ballToGoal * behindBallDist;
                        float carToTargetDist = Vector2.Distance(transform.position, targetPos);
                        float reachTargetDist = 0.3f;
                        if (carToTargetDist > reachTargetDist)
                        {
                            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
                            rb.AddForce(dir * _acceleration * Time.deltaTime);
                        }
                        else
                        {
                            Vector2 pushDir = (enemyGoal.position - ball.position).normalized;
                            rb.AddForce(pushDir * _acceleration * Time.deltaTime);
                        }
                        break;
                    }
                case AIState.Reversing:
                    {
                        // Go backward from ball
                        Vector2 awayFromBall = (transform.position - ball.position).normalized;
                        rb.AddForce(awayFromBall * _acceleration * 1.2f * Time.deltaTime); // a bit more force
                        stateTimer -= Time.deltaTime;
                        if (stateTimer <= 0f)
                        {
                            state = AIState.Charging;
                            stateTimer = 0.4f; // charge for 0.4s
                        }
                        break;
                    }
                case AIState.Charging:
                    {
                        // Move quickly toward ball
                        Vector2 toBall = (ball.position - transform.position).normalized;
                        rb.AddForce(toBall * _acceleration * 2.5f * Time.deltaTime); // much more force
                        stateTimer -= Time.deltaTime;
                        if (stateTimer <= 0f)
                        {
                            state = AIState.Pushing;
                        }
                        break;
                    }
            }
        }
        else if (easyMode)
        {
            float easyAcceleration = _acceleration;
            float easyTurnSpeed = turnSpeed;

            Vector2 ballToGoal = (enemyGoal.position - ball.position).normalized;
            Vector2 side = Vector2.Perpendicular(ballToGoal);
            Vector2 sideTarget = (Vector2)ball.position + side * 1.0f * (Random.value < 0.5f ? 1f : -1f);

            float carToBallDist = Vector2.Distance(transform.position, ball.position);

            Vector2 target;
            float rotationSpeed = 0f;
            if (carToBallDist > 0.7f)
            {
                target = sideTarget;
                rotationSpeed = easyTurnSpeed * 0.2f; // EVEN SLOWER turn for smoothness
            }
            else
            {
                target = (Vector2)ball.position + ballToGoal * 1.5f;
                rotationSpeed = easyTurnSpeed * 0.7f; // Slightly slower than hard for smooth push
            }

            Vector2 toTarget = (target - (Vector2)transform.position);
            float distToTarget = toTarget.magnitude;
            Vector2 toTargetDir = toTarget.normalized;

            float angle = Mathf.Atan2(toTargetDir.y, toTargetDir.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float newAngle = currentAngle;

            // Only rotate if target is not too close
            if (distToTarget > 0.15f)
            {
                // Use Mathf.MoveTowardsAngle for smooth, frame-independent rotation
                newAngle = Mathf.MoveTowardsAngle(currentAngle, angle, rotationSpeed * Time.deltaTime);
                rb.MoveRotation(newAngle);
            }
            // else: do not rotate, keep current angle

            // Move toward target (same as hard)
            rb.AddForce(toTargetDir * easyAcceleration * Time.deltaTime);
        }
       
    }


}