using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject platformPrefab;

    [Header("Platform")]
    [SerializeField] private Vector3 platformSize = new Vector3(3f, 1f, 3f);
    [SerializeField] private bool forceUniformSize = false;
    [SerializeField] private bool randomizeSize = true;
    [SerializeField] private Vector2 widthRange = new Vector2(2.5f, 6f);
    [SerializeField] private Vector2 depthRange = new Vector2(2.5f, 7f);
    [SerializeField] private float platformSpeed = 6f;
    [SerializeField] private float spawnDistance = 25f;
    [SerializeField] private float spawnY = 0f;
    [SerializeField] private float xRange = 0f;
    [SerializeField] private float minGap = 4f;
    [SerializeField] private float maxGap = 9f;
    [SerializeField] private float startSafeGap = 10f;
    [SerializeField] private int prewarmCount = 4;
    [SerializeField] private bool spawnStartPlatform = true;
    [SerializeField] private bool useLongStartPlatform = true;
    [SerializeField] private float startPlatformLength = 16f;
    [SerializeField] private float startPlatformWidth = 7f;

    [Header("Despawn")]
    [SerializeField] private float despawnBehind = 12f;

    [Header("Style")]
    [SerializeField] private bool forcePrimitivePlatforms = true;
    [SerializeField] private bool addDecorations = false;
    [SerializeField, Range(0, 100)] private int decorationChance = 0;
    [SerializeField] private Vector2 decorationScaleRange = new Vector2(0.4f, 1.1f);
    [SerializeField] private Color[] colorPalette;
    [SerializeField] private bool forceGray = false;
    [SerializeField] private Transform platformParent;

    private float _spawnTimer;
    private float _nextSpawnDelay;
    private float _avgDepth;
    private Material[] _paletteMaterials;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (platformParent == null)
        {
            GameObject container = new GameObject("Platforms");
            container.transform.SetParent(transform, false);
            platformParent = container.transform;
        }

        if (forceUniformSize)
        {
            randomizeSize = false;
        }

        if (forcePrimitivePlatforms)
        {
            platformPrefab = null;
        }

        addDecorations = false;
        decorationChance = 0;

        InitStyle();
        _avgDepth = GetAverageDepth();

        SpawnInitialPlatforms();
    }

    private void Update()
    {
        if (platformSpeed <= 0.01f)
        {
            return;
        }

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _nextSpawnDelay)
        {
            _spawnTimer = 0f;
            SpawnPlatform(GetSpawnPosition(GetPlayerZ() + spawnDistance));
            ScheduleNextSpawn();
        }
    }

    private void PrewarmPlatforms()
    {
        float playerZ = GetPlayerZ();
        float z = playerZ + spawnDistance + startSafeGap;
        float minDepth = GetMinDepth();

        for (int i = 0; i < prewarmCount; i++)
        {
            Vector3 size = GetPlatformSize();
            SpawnPlatform(GetSpawnPosition(z), size);
            float gap = Random.Range(minGap, maxGap);
            z -= size.z + gap;
            if (z <= playerZ + startSafeGap + minDepth * 0.5f)
            {
                break;
            }
        }
    }

    private void ScheduleNextSpawn()
    {
        ScheduleNextSpawn(0f);
    }

    private void ScheduleNextSpawn(float extraDistance)
    {
        float gap = Random.Range(minGap, maxGap);
        float spacing = _avgDepth + gap + Mathf.Max(0f, extraDistance);
        _nextSpawnDelay = spacing / Mathf.Max(0.01f, platformSpeed);
    }

    private Vector3 GetSpawnPosition(float z)
    {
        float x = xRange > 0f ? Random.Range(-xRange, xRange) : 0f;
        return new Vector3(x, spawnY, z);
    }

    private float GetPlayerZ()
    {
        return player != null ? player.position.z : 0f;
    }

    private void SpawnPlatform(Vector3 position)
    {
        Vector3 size = GetPlatformSize();
        SpawnPlatform(position, size);
    }

    private void SpawnPlatform(Vector3 position, Vector3 size)
    {
        GameObject platform = (!forcePrimitivePlatforms && platformPrefab != null)
            ? Instantiate(platformPrefab, position, Quaternion.identity)
            : CreatePlatform(position);

        platform.transform.localScale = size;
        if (platformParent != null)
        {
            platform.transform.SetParent(platformParent, true);
        }

        Material material = GetRandomMaterial();
        ApplyMaterial(platform, material);

        PlatformMover mover = platform.GetComponent<PlatformMover>();
        if (mover == null)
        {
            mover = platform.AddComponent<PlatformMover>();
        }
        mover.Speed = platformSpeed;
        mover.Direction = Vector3.back;

        PlatformDespawn despawn = platform.GetComponent<PlatformDespawn>();
        if (despawn == null)
        {
            despawn = platform.AddComponent<PlatformDespawn>();
        }
        despawn.Configure(player, despawnBehind);
    }

    public void ResetSpawner()
    {
        if (platformParent != null)
        {
            for (int i = platformParent.childCount - 1; i >= 0; i--)
            {
                Destroy(platformParent.GetChild(i).gameObject);
            }
        }

        SpawnInitialPlatforms();
    }

    private void SpawnInitialPlatforms()
    {
        _spawnTimer = 0f;

        if (spawnStartPlatform)
        {
            SpawnStartPlatform();
        }

        PrewarmPlatforms();
        ScheduleNextSpawn(startSafeGap);
    }

    private void SpawnStartPlatform()
    {
        Vector3 size = useLongStartPlatform ? GetStartPlatformSize() : GetPlatformSize();
        SpawnPlatform(GetSpawnPosition(GetPlayerZ()), size);
    }

    private Vector3 GetPlatformSize()
    {
        if (forceUniformSize || !randomizeSize)
        {
            return platformSize;
        }

        float width = Mathf.Max(0.5f, Random.Range(widthRange.x, widthRange.y));
        float depth = Mathf.Max(0.5f, Random.Range(depthRange.x, depthRange.y));
        return new Vector3(width, platformSize.y, depth);
    }

    private Vector3 GetStartPlatformSize()
    {
        if (forceUniformSize)
        {
            return platformSize;
        }

        float baseWidth = randomizeSize ? Mathf.Max(0.5f, (widthRange.x + widthRange.y) * 0.5f) : platformSize.x;
        float width = Mathf.Max(baseWidth, startPlatformWidth);
        float depth = Mathf.Max(0.5f, startPlatformLength);
        return new Vector3(width, platformSize.y, depth);
    }

    private float GetAverageDepth()
    {
        if (forceUniformSize || !randomizeSize)
        {
            return platformSize.z;
        }

        return Mathf.Max(0.5f, (depthRange.x + depthRange.y) * 0.5f);
    }

    private float GetMinDepth()
    {
        if (forceUniformSize || !randomizeSize)
        {
            return platformSize.z;
        }

        return Mathf.Max(0.5f, Mathf.Min(depthRange.x, depthRange.y));
    }

    private GameObject CreatePlatform(Vector3 position)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.position = position;

        Rigidbody rb = platform.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = platform.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        return platform;
    }

    private void InitStyle()
    {
        if (forceGray)
        {
            colorPalette = new[]
            {
                new Color(0.55f, 0.55f, 0.55f)
            };
        }
        else if (colorPalette == null || colorPalette.Length == 0)
        {
            colorPalette = new[]
            {
                new Color(0.85f, 0.15f, 0.15f), // red
                new Color(0.25f, 0.25f, 0.85f), // blue
                new Color(0.70f, 0.20f, 0.80f), // purple
                new Color(0.80f, 0.25f, 0.55f), // magenta
                new Color(0.55f, 0.55f, 0.55f)  // gray
            };
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        _paletteMaterials = new Material[colorPalette.Length];
        for (int i = 0; i < colorPalette.Length; i++)
        {
            Material mat = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
            mat.color = colorPalette[i];

            _paletteMaterials[i] = mat;
        }
    }

    private Material GetRandomMaterial()
    {
        if (_paletteMaterials == null || _paletteMaterials.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, _paletteMaterials.Length);
        return _paletteMaterials[index];
    }

    private void ApplyMaterial(GameObject target, Material material)
    {
        if (material == null)
        {
            return;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }

    private void TryAddDecoration(GameObject platform, Material material)
    {
        if (!addDecorations || Random.Range(0, 100) >= decorationChance)
        {
            return;
        }

        PrimitiveType type = Random.value < 0.5f ? PrimitiveType.Cylinder : PrimitiveType.Cube;
        GameObject deco = GameObject.CreatePrimitive(type);
        deco.name = "Decoration";
        deco.transform.SetParent(platform.transform, false);

        float scale = Random.Range(decorationScaleRange.x, decorationScaleRange.y);
        Vector3 half = platform.transform.localScale * 0.5f;
        float x = Random.Range(-half.x * 0.6f, half.x * 0.6f);
        float z = Random.Range(-half.z * 0.6f, half.z * 0.6f);
        deco.transform.localPosition = new Vector3(x, half.y + scale * 0.5f, z);
        deco.transform.localScale = new Vector3(scale, scale, scale);

        Collider decoCollider = deco.GetComponent<Collider>();
        if (decoCollider != null)
        {
            Destroy(decoCollider);
        }

        ApplyMaterial(deco, material);
    }

    private Texture2D CreateCheckerTexture(Color baseColor)
    {
        Color dark = Color.Lerp(baseColor, Color.black, 0.2f);
        Color light = Color.Lerp(baseColor, Color.white, 0.2f);

        Texture2D tex = new Texture2D(4, 4);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                bool isLight = (x + y) % 2 == 0;
                tex.SetPixel(x, y, isLight ? light : dark);
            }
        }

        tex.Apply();
        return tex;
    }
}
