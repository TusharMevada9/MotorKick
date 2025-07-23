using UnityEngine;

public class CarController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    public float boostMultiplier = 2f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");
        float turn = -Input.GetAxis("Horizontal");
        bool boosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space);

        float speed = boosting ? moveSpeed * boostMultiplier : moveSpeed;

        rb.AddForce(transform.up * move * speed);
        rb.AddTorque(turn * turnSpeed * Time.fixedDeltaTime);
    }
}
