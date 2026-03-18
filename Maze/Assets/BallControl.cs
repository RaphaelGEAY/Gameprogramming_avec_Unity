using UnityEngine;

public class BallControl : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 1.0f;
    public Transform respawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // void Update()
    // {
    //     float moveX = Input.GetAxis("Horizontal");
    //     float moveZ = Input.GetAxis("Vertical");

    //     Vector3 force = new Vector3(moveX, 0, moveZ);

    //     rb.AddForce(force * speed);

    // }

    private void OnTriggerEnter(Collider other){
        Debug.Log("Trigger Enter:" + other.gameObject.tag);
        if (other.gameObject.tag == "FinishZone") {
            rb.isKinematic = true;
            transform.position = respawnPoint.position;
            rb.isKinematic = false;
        }
    }
}
