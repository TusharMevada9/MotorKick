using UnityEngine;

public class BallController : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Optional: Reset ball position
    public void ResetBall(Vector2 position)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = position;
    }

    // Optional: Add more features (goal detection, effects, etc.)

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the other object is a car (by tag or component)
        // Option 1: By tag (recommended to tag all car objects as 'Car')
        if (collision.gameObject.CompareTag("Car") || collision.gameObject.GetComponent<CarBehavior>() != null)
        {
            // Calculate direction from car to ball
            Vector2 forceDir = (rb.position - (Vector2)collision.transform.position).normalized;
            float forceMag = collision.relativeVelocity.magnitude * 0.01f; // Tune multiplier for effect
            rb.AddForce(forceDir * forceMag, ForceMode2D.Impulse);
        }
    }
}
