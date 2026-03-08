using UnityEngine;

public class DamageBlock : MonoBehaviour
{
    public float damageAmount = 20f;
    public float damageCooldown = 1.0f; // 伤害间隔秒数
    private float lastDamageTime;

    private void OnCollisionEnter(Collision collision)
    {
        // 检查碰撞的是不是玩家
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // 检查冷却时间
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                // 这里视为 Trap 类型伤害
                playerHealth.TakeDamage(damageAmount, DeathCause.Trap);
                lastDamageTime = Time.time;
            }
        }
    }
}