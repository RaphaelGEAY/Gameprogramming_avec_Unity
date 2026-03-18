using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        RunManager manager = RunManager.Instance != null ? RunManager.Instance : FindFirstObjectByType<RunManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("RunManager");
            manager = managerObj.AddComponent<RunManager>();
        }
        if (manager != null)
        {
            manager.ResetRun();
        }
    }
}
