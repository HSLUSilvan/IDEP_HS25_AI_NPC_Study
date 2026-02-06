using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Animator Parameters")]
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string speedParam = "Speed";

    [Header("4-Direction Only")]
    [SerializeField] private bool snapToCardinal = true;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Prevent physics rotation
        rb.freezeRotation = true;

        // Initial facing direction (idle down)
        animator.SetFloat(moveXParam, lastFacing.x);
        animator.SetFloat(moveYParam, lastFacing.y);
        animator.SetFloat(speedParam, 0f);
    }

    // Called by Input System
    public void OnMove(InputValue value)
    {
        Debug.Log("[TopDownPlayerController] OnMove called: " + value.Get<Vector2>());
        moveInput = value.Get<Vector2>();

        if (snapToCardinal)
            moveInput = SnapToCardinal(moveInput);

        float speed = moveInput.magnitude;

        animator.SetFloat(speedParam, speed);

        if (speed > 0.01f)
        {
            Vector2 dir = moveInput.normalized;
            lastFacing = dir;

            animator.SetFloat(moveXParam, dir.x);
            animator.SetFloat(moveYParam, dir.y);
        }
        else
        {
            // Keep facing direction when idle
            animator.SetFloat(moveXParam, lastFacing.x);
            animator.SetFloat(moveYParam, lastFacing.y);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private static Vector2 SnapToCardinal(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector2.zero;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return new Vector2(Mathf.Sign(input.x), 0f);

        return new Vector2(0f, Mathf.Sign(input.y));
    }
}
