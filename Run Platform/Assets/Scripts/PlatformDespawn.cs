using UnityEngine;

public class PlatformDespawn : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float despawnBehind = 12f;
    [SerializeField] private float minLifeTime = 1f;

    private Camera _camera;
    private float _spawnTime;

    private void Start()
    {
        _spawnTime = Time.time;
        _camera = Camera.main;

        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    public void Configure(Transform newTarget, float newDespawnBehind)
    {
        target = newTarget;
        despawnBehind = newDespawnBehind;
    }

    private void Update()
    {
        if (Time.time - _spawnTime < minLifeTime)
        {
            return;
        }

        if (_camera != null)
        {
            Vector3 toObject = transform.position - _camera.transform.position;
            if (Vector3.Dot(_camera.transform.forward, toObject) < 0f)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (target != null && transform.position.z < target.position.z - despawnBehind)
        {
            Destroy(gameObject);
        }
    }
}
