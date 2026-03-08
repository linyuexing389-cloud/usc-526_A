using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public float healAmount = 20f;

    private void OnTriggerEnter(Collider other)
    {
        // 确保碰撞体是玩家
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.RestoreHealth(healAmount); // 调用回血
                Destroy(gameObject); // 吃完消失
            }
        }
    }
}