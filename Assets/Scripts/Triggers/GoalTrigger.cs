using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入触发区的是不是小球（必须设置 Tag 为 Player）
        if (other.CompareTag("Player"))
        {
            // 调用 GameManager 的通关逻辑
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }
}