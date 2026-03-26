using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("关卡逻辑")]
    [Tooltip("如果是最后一关，请勾选此项；否则会尝试加载下一关")]
    public bool isFinalLevel = true; // 默认设为 true，符合你现在的需求
    public string nextSceneName = "cube_map 1";

    [Header("倒计时设置")]
    public float timeRemaining = 120f;
    private bool isGameOver = false;

    [Header("UI 引用")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText; // WIN 时用

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1;
        if (statusText != null) statusText.text = "";
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

            // 记录一次超时死亡（没有直接 HP 变化）
            float hpRemaining = -1f;
            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null)
            {
                hpRemaining = player.currentHealth;
            }
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

    // --- 核心逻辑修改点 ---

    public void WinGame()
    {
        if (isGameOver) return;

        if (isFinalLevel)
        {
            // 如果是最后一关：显示胜利文字，停止游戏
            EndGame("WIN");
        }
        else
        {
            // 如果不是最后一关：确保时间流动并跳转场景
            Time.timeScale = 1; 
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        EndGame("LOSE");
    }

    private void EndGame(string message)
    {
        isGameOver = true;
        
        // 游戏结束时停止时间（如果是胜利，玩家可以停下来欣赏战果）
        Time.timeScale = 0; 

        if (message == "LOSE")
        {
            DeathAnalyticsGraphUI.ShowOnGameOver();
        }
        else if (statusText != null)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(true);
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1;
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
            yield return new WaitForSeconds(interval);
            timerText.alpha = 1f;
            yield return new WaitForSeconds(interval);
            elapsed += interval * 2f;
        }
        timerText.alpha = 1f;
        _flashCoroutine = null;
    }
}