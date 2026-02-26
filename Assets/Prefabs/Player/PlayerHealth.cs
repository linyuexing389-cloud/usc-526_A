using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI 引用")]
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI(); // 初始化 UI 状态
    }

    // --- 1. 受伤逻辑 (保留并优化了你的原始逻辑) ---
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    // --- 2. 回血逻辑 (新增功能) ---
    public void RestoreHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();
        Debug.Log($"吃到血包！当前血量: {currentHealth}");
    }

    // --- 3. UI 更新中心 (整合了你之前的 FillRect 修复逻辑) ---
    private void UpdateUI()
{
    if (healthSlider != null)
    {
        Debug.Log($"准备更新UI，当前血量：{currentHealth}，Slider当前值：{healthSlider.value}"); // 加这句
        healthSlider.value = currentHealth;
        
        if (healthSlider.fillRect != null)
        {
            healthSlider.fillRect.gameObject.SetActive(currentHealth > 0.1f);
        }
    }
    else
    {
        Debug.LogError("警告：PlayerHealth 找不到 Slider 引用！"); // 加这句
    }
}

    private void Die()
    {
        Debug.Log("似了喵~");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame();
        }
    }
}