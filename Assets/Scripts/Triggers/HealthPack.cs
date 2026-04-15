using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public float healAmount = 20f;

    private void OnTriggerEnter(Collider other)
    {
        // 确保碰撞体是玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("碰到玩家了");
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            Debug.Log(health.ToString());
            if (health != null)
            {
                health.RestoreHealth(healAmount); // 调用回血
                PickupPopupText.Show(transform.position, $"+{Mathf.RoundToInt(healAmount)}", new Color(0.82f, 0.18f, 0.18f));
                Destroy(gameObject); // 吃完消失
            }
        }
    }
}
