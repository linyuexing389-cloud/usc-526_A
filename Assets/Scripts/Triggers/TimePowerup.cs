using UnityEngine;

public class TimePowerup : MonoBehaviour
{
    [Header("设置")]
    public float bonusTime = 30f; // 增加多少秒

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

            PickupPopupText.Show(transform.position, $"+{Mathf.RoundToInt(bonusTime)} s", new Color(0.08f, 0.08f, 0.08f));

            // 2. 播放拾取音效
            SoundManager.PlayHealthTime(transform.position);

            // 3. 销毁胶囊自己
            Destroy(gameObject);
        }
    }
}
