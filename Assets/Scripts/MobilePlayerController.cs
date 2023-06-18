using UnityEngine;

public class MobilePlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 lastDirection;

    MovementJoystick js;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        js = FindObjectOfType<MovementJoystick>();
    }

    private void FixedUpdate()
    {
        Vector2 input = js.joystickVec;

        if (input != Vector2.zero)
        {
            rb.velocity = input * speed;
            lastDirection = input;
        }
        else
        {
            rb.velocity = lastDirection * speed;
        }
    }
}
