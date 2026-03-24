using UnityEngine;

public class TimePowerup : MonoBehaviour
{
    [Header("设置")]
    public float bonusTime = 10f; // 增加多少秒

    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞的是不是小球
        if (other.CompareTag("Player"))
        {
            // 1. 调用 GameManager 增加时间
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddTime(bonusTime);
            }

            // 2. 销毁胶囊自己
            Destroy(gameObject);
            
            // 提示：如果你想加个吃掉的声音，可以在 Destroy 之前写播放逻辑
        }
    }
}