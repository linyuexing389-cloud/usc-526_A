using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 确保包含 UI 命名空间

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("关卡逻辑")]
    [Tooltip("如果是最后一关，请勾选此项；否则会尝试加载下一关")]
    public bool isFinalLevel = true; 
    public string nextSceneName = "cube_map 1";

    [Header("倒计时设置")]
    public float timeRemaining = 120f;
    private bool isGameOver = false;
    private bool isPaused = false;

    [Header("UI 引用")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText; 

    private GameObject pauseMenuRoot;
    private Text pauseTitleText;
    private Button resumeButton;
    private Button retryButton;
    private Button mainMenuButton;

    private void Awake()
    {
        // [修复点 1] 核心：确保每次场景加载时，时间流逝立刻恢复正常
        Time.timeScale = 1f;
        
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (statusText != null) statusText.text = "";

        EnsurePauseMenu();
        HidePauseMenu();

        DeathAnalyticsManager.EnsureInstance();
        if (BetaAnalyticsManager.Instance != null)
            BetaAnalyticsManager.Instance.BeginLevelSession();
    }

    void Update()
    {
        if (!isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (isGameOver) return;
        if (isPaused) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay(timeRemaining);
        }
        else
        {
            timeRemaining = 0;

            float hpRemaining = -1f;
            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null)
            {
                hpRemaining = player.currentHealth;
            }
            // 确保 DeathAnalyticsManager 已经定义并存在
            DeathAnalyticsManager.LogDeath(DeathCause.Timeout, timeRemaining, hpRemaining);

            LoseGame();
        }
    }

    void UpdateTimerDisplay(float time)
    {
        if (timerText == null) return;
        float displayTime = Mathf.Max(0, time); 
        float minutes = Mathf.FloorToInt(displayTime / 60);
        float seconds = Mathf.FloorToInt(displayTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = (time <= 30f) ? Color.red : Color.white;
    }

    public void WinGame()
    {
        if (isGameOver) return;
        EndGame("WIN");
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        EndGame("LOSE");
    }

    private void EndGame(string message)
    {
        isGameOver = true;
        HidePauseMenu();

        if (BetaAnalyticsManager.Instance != null)
            BetaAnalyticsManager.Instance.LogSessionEnd(message == "WIN");

        if (message == "WIN" && SceneManager.GetActiveScene().name == "tutorial_level")
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }

        // 游戏结束时停止时间
        Time.timeScale = 0f;

        // 确保 DeathAnalyticsGraphUI 已经定义并存在
        DeathAnalyticsGraphUI.ShowOnGameOver(isWin: message == "WIN");
    }

    public void RetryGame()
    {
        // [修复点 2] 在加载新场景前，强制恢复时间并停止所有可能卡住的协程
        isPaused = false;
        Time.timeScale = 1f;
        StopAllCoroutines(); 
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResumeGame()
    {
        if (isGameOver) return;
        HidePauseMenu();
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        StopAllCoroutines();
        SceneManager.LoadScene("Scene_MainMenu");
    }

    public void AddTime(float amount)
    {
        if (isGameOver) return;
        timeRemaining += amount;
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashTimer(3f));
    }

    private Coroutine _flashCoroutine;
    private IEnumerator FlashTimer(float duration)
    {
        float elapsed = 0f;
        const float interval = 0.15f;
        while (elapsed < duration)
        {
            timerText.alpha = 0f;
            // [修复点 3] 使用 WaitForSecondsRealtime 避免受 timeScale 变为 0 的影响
            yield return new WaitForSecondsRealtime(interval);
            timerText.alpha = 1f;
            yield return new WaitForSecondsRealtime(interval);
            elapsed += interval * 2f;
        }
        timerText.alpha = 1f;
        _flashCoroutine = null;
    }

    private void TogglePauseMenu()
    {
        if (isPaused) HidePauseMenu();
        else ShowPauseMenu();
    }

    private void ShowPauseMenu()
    {
        EnsurePauseMenu();
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);
    }

    private void HidePauseMenu()
    {
        isPaused = false;
        if (!isGameOver)
            Time.timeScale = 1f;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }

    private void EnsurePauseMenu()
    {
        if (pauseMenuRoot != null) return;

        var canvasGo = new GameObject("PauseMenuCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        pauseMenuRoot = new GameObject("PauseMenu");
        pauseMenuRoot.transform.SetParent(canvasGo.transform, false);
        var rootRect = pauseMenuRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(760, 520);
        rootRect.anchoredPosition = Vector2.zero;

        var bg = pauseMenuRoot.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.07f, 0.12f, 0.96f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        pauseTitleText = AddMenuText(pauseMenuRoot, "Paused", 78, FontStyle.Bold, new Vector2(0, 165), new Vector2(520, 100), font);
        pauseTitleText.color = new Color(0.95f, 0.95f, 0.98f);

        CreateMenuButton(pauseMenuRoot, "ResumeButton", "Resume", new Color(0.22f, 0.55f, 0.28f), new Vector2(0, 50), font, out resumeButton);
        CreateMenuButton(pauseMenuRoot, "RetryButton", "Restart Level", new Color(0.28f, 0.36f, 0.7f), new Vector2(0, -45), font, out retryButton);
        CreateMenuButton(pauseMenuRoot, "MainMenuButton", "Main Menu", new Color(0.42f, 0.24f, 0.22f), new Vector2(0, -140), font, out mainMenuButton);

        resumeButton.onClick.AddListener(ResumeGame);
        retryButton.onClick.AddListener(RetryGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private static void CreateMenuButton(GameObject parent, string buttonName, string labelText, Color bgColor, Vector2 anchoredPos, Font font, out Button button)
    {
        var buttonGo = new GameObject(buttonName);
        buttonGo.transform.SetParent(parent.transform, false);

        var rect = buttonGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(340, 72);
        rect.anchoredPosition = anchoredPos;

        var image = buttonGo.AddComponent<Image>();
        image.color = bgColor;
        buttonGo.AddComponent<CanvasRenderer>();

        button = buttonGo.AddComponent<Button>();
        button.targetGraphic = image;

        var colors = button.colors;
        colors.highlightedColor = new Color(
            Mathf.Clamp01(bgColor.r + 0.1f),
            Mathf.Clamp01(bgColor.g + 0.1f),
            Mathf.Clamp01(bgColor.b + 0.1f));
        colors.pressedColor = new Color(
            Mathf.Clamp01(bgColor.r - 0.08f),
            Mathf.Clamp01(bgColor.g - 0.08f),
            Mathf.Clamp01(bgColor.b - 0.08f));
        button.colors = colors;

        var label = AddMenuText(buttonGo, labelText, 30, FontStyle.Bold, Vector2.zero, new Vector2(340, 72), font);
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
    }

    private static Text AddMenuText(GameObject parent, string content, int fontSize, FontStyle style, Vector2 pos, Vector2 size, Font font)
    {
        var go = new GameObject("Text_" + content);
        go.transform.SetParent(parent.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;

        go.AddComponent<CanvasRenderer>();
        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        if (font != null) text.font = font;
        return text;
    }
}
