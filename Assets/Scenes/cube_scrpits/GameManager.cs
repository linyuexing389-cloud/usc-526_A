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

    [Header("UI 引用")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText; 

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

        DeathAnalyticsManager.EnsureInstance();
        if (BetaAnalyticsManager.Instance != null)
            BetaAnalyticsManager.Instance.BeginLevelSession();
    }

    void Update()
    {
        if (isGameOver) return;

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

        if (BetaAnalyticsManager.Instance != null)
            BetaAnalyticsManager.Instance.LogSessionEnd(message == "WIN");

        // 游戏结束时停止时间
        Time.timeScale = 0f; 

        // 确保 DeathAnalyticsGraphUI 已经定义并存在
        DeathAnalyticsGraphUI.ShowOnGameOver(isWin: message == "WIN");
    }

    public void RetryGame()
    {
        // [修复点 2] 在加载新场景前，强制恢复时间并停止所有可能卡住的协程
        Time.timeScale = 1f;
        StopAllCoroutines(); 
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}