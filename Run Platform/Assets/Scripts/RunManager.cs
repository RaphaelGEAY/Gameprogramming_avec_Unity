using UnityEngine;
using UnityEngine.UI;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlatformSpawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private Transform startPoint;

    [Header("Score")]
    [SerializeField] private float scoreRate = 1f;

    [Header("Death")]
    [SerializeField] private bool useHeightDeath = true;
    [SerializeField] private float deathY = -5f;
    [SerializeField] private float resetCooldown = 0.25f;

    private float _score;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _playerRb;
    private bool _initialized;
    private float _lastResetTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetRun();
    }

    private void Update()
    {
        _score += scoreRate * Time.deltaTime;
        UpdateScoreText();

        if (useHeightDeath && player != null && Time.time - _lastResetTime > resetCooldown)
        {
            if (player.position.y < deathY)
            {
                ResetRun();
            }
        }
    }

    public void ResetRun()
    {
        EnsureInitialized();
        _lastResetTime = Time.time;
        _score = 0f;
        UpdateScoreText();

        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        if (player != null)
        {
            player.position = _startPosition;
            player.rotation = _startRotation;

            if (_playerRb != null)
            {
                _playerRb.linearVelocity = Vector3.zero;
                _playerRb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Mathf.FloorToInt(_score)}";
        }
    }

    private void EnsureScoreUI()
    {
        if (scoreText != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HUD");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(canvas.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -20f);
        rect.sizeDelta = new Vector2(260f, 60f);

        scoreText = text;
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (spawner == null)
        {
            spawner = FindFirstObjectByType<PlatformSpawner>();
        }

        if (player != null)
        {
            if (startPoint != null)
            {
                _startPosition = startPoint.position;
                _startRotation = startPoint.rotation;
            }
            else
            {
                PlayerJumpController jump = player.GetComponent<PlayerJumpController>();
                if (jump != null)
                {
                    _startPosition = jump.StartPosition;
                    _startRotation = jump.StartRotation;
                }
                else
                {
                    _startPosition = player.position;
                    _startRotation = player.rotation;
                }
            }
            _playerRb = player.GetComponent<Rigidbody>();
        }

        EnsureScoreUI();
        _initialized = true;
    }
}
