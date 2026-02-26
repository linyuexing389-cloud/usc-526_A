using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    public float damageAmount = 10f;
    public float cooldown = 0.5f; // 穿过时的伤害间隔，防止瞬间秒杀
    private float nextDamageTime;

    private void OnTriggerStay(Collider other)
    {
        // 只有碰到小球（Player）才扣血
        if (other.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                // 调用你之前的血量脚本
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(damageAmount);
                    nextDamageTime = Time.time + cooldown;
                }
            }
        }
    }
}