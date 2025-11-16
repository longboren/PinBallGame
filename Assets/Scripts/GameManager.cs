using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public Text scoreText;
    public Transform ballSpawnPoint;

    private GameObject currentBall;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CreateScoreUI();
        UpdateScoreUI();
    }

    void CreateScoreUI()
    {
        // Create UI Canvas if it doesn't exist
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create Score Text if it doesn't exist
        if (scoreText == null)
        {
            GameObject textObj = new GameObject("ScoreText");
            textObj.transform.SetParent(canvas.transform, false);
            
            scoreText = textObj.AddComponent<Text>();
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 36;
            scoreText.color = Color.white;
            scoreText.alignment = TextAnchor.UpperLeft;
            scoreText.text = "Score: 0";
            
            // Position at top-left
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(20, -20);
            rectTransform.sizeDelta = new Vector2(300, 50);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void RegisterBall(GameObject ball)
    {
        currentBall = ball;
        CameraFollow cf = Camera.main?.GetComponent<CameraFollow>();
        if (cf && ball != null) cf.target = ball.transform;
    }

    public void BallDrained(GameObject ball)
    {
        if (ball == currentBall)
        {
            StartCoroutine(HandleDrain(ball));
        }
    }

    IEnumerator HandleDrain(GameObject ball)
    {
        // brief delay
        yield return new WaitForSeconds(0.5f);

        if (ball != null) Destroy(ball);
        currentBall = null;

        // spawn new ball after short delay
        yield return new WaitForSeconds(1f);
        SpawnNewBall();
    }

    public void SpawnNewBall()
    {
        Vector3 spawn = ballSpawnPoint != null ? ballSpawnPoint.position : new Vector3(0f, 0.6f, 0.8f);
        if (FindObjectOfType<PinballTableGenerator>() != null)
        {
            // ask generator to spawn via simple method. If you prefer, call its SpawnBall directly.
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.position = spawn;
            ball.transform.localScale = Vector3.one * 0.3f;
            ball.tag = "Ball";
            Rigidbody rb = ball.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            GameManager.Instance.RegisterBall(ball);
        }
    }

    public void OnBallLaunched()
    {
        // placeholder for combos, UI, etc.
    }
}