using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private Vector3 direction = Vector3.back;

    private Rigidbody _rb;

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    public Vector3 Direction
    {
        get => direction;
        set => direction = value;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 delta = direction.normalized * speed * Time.fixedDeltaTime;
        if (_rb != null && _rb.isKinematic)
        {
            _rb.MovePosition(_rb.position + delta);
        }
        else
        {
            transform.position += delta;
        }
    }
}
