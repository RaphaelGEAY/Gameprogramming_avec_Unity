using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerJumpController : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float groundCheckExtra = 0.1f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody _rb;
    private Collider _collider;

    public Vector3 StartPosition { get; private set; }
    public Quaternion StartRotation { get; private set; }

    private void Awake()
    {
        StartPosition = transform.position;
        StartRotation = transform.rotation;

        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        if (mass > 0f)
        {
            _rb.mass = mass;
        }
        _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Update()
    {
        if (IsJumpPressed() && IsGrounded())
        {
            Jump();
        }
    }

    private bool IsGrounded()
    {
        if (_collider == null)
        {
            return Physics.Raycast(transform.position, Vector3.down, 0.2f, groundLayers, QueryTriggerInteraction.Ignore);
        }

        Vector3 origin = _collider.bounds.center;
        float distance = _collider.bounds.extents.y + groundCheckExtra;
        return Physics.Raycast(origin, Vector3.down, distance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void Jump()
    {
        Vector3 velocity = _rb.linearVelocity;
        float gravity = Physics.gravity.y;
        float jumpSpeed = Mathf.Sqrt(Mathf.Max(0.01f, -2f * gravity * jumpHeight));
        velocity.y = jumpSpeed;
        _rb.linearVelocity = velocity;
    }

    private bool IsJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
    }
}
