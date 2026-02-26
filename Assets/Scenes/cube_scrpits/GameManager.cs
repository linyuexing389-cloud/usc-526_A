using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("倒计时设置")]
    public float timeRemaining = 300f;
    private bool isGameOver = false;

    [Header("UI 引用")]
    public TextMeshProUGUI timerText;    // 顶部的倒计时
    public TextMeshProUGUI statusText;   // 屏幕中间的状态文字

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 游戏开始时确保时间正常流动，状态文字为空
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
            LoseGame();
        }
    }

     void UpdateTimerDisplay(float time)
{
    if (timerText == null) return;

    // 修复点：强制不小于 0，防止出现 -01:-01
    float displayTime = Mathf.Max(0, time); 

    float minutes = Mathf.FloorToInt(displayTime / 60);
    float seconds = Mathf.FloorToInt(displayTime % 60);
    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
}

    // --- 游戏结束逻辑 ---

    public void WinGame()
    {
        if (isGameOver) return;
        EndGame("win");
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        EndGame("lose");
    }

    private void EndGame(string message)
    {
        isGameOver = true;
        
        // 停止游戏时间（这会停止物理模拟和所有依赖 Time.deltaTime 的逻辑）
        Time.timeScale = 0; 

        // 显示状态文字
        if (statusText != null)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(true);
        }
        
        Debug.Log("Game Over: " + message);
    }
    // 在 GameManager.cs 中添加这个方法
    public void AddTime(float amount)
    {
        if (isGameOver) return; // 如果游戏已经结束（显示了文字），就不加时间了
        
        timeRemaining += amount;
        
        // 可选：为了让玩家反馈更明显，你可以在这里让倒计时文字闪烁一下
        Debug.Log("增加时间: " + amount + "s");
    }
}