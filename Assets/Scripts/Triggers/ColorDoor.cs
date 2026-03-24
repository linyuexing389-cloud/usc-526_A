using UnityEngine;

public class ColorDoor : MonoBehaviour
{
    public string requiredColor; // 这扇门需要的颜色标识

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerInventory inventory = collision.gameObject.GetComponent<PlayerInventory>();
            
            // 检查背包是否为空，并且是否有该颜色的钥匙
            if (inventory != null && inventory.HasKey(requiredColor))
            {
                Debug.Log($"使用 {requiredColor} 钥匙，门开了！");
                
                // 【核心修改】：消耗掉一把对应颜色的钥匙
                inventory.UseKey(requiredColor); 
                
                OpenDoor();
            }
            else
            {
                Debug.Log($"你需要一把 {requiredColor} 钥匙才能通过。");
                // 这里可以加一个“门晃动”或“锁定音效”的反馈
            }
        }
    }

    void OpenDoor()
    {
        // 简单处理：门直接消失
        // 进阶处理：可以播放开门动画或移动门的位置
        Destroy(gameObject); 
    }
}