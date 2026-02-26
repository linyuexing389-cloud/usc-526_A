using UnityEngine;

public class MapRotator : MonoBehaviour
{
    [Header("旋转设置")]
    public float lerpSpeed = 10f;       // 旋转平滑度，越大转得越快
    private float targetZAngle = 0f;    // 目标 Z 轴角度

    [Header("物理辅助 (拖入小球)")]
    public Rigidbody ballRigidbody;     // 在 Inspector 面板里把你的小球拖到这里
    public float startKickForce = 0.5f; // 起步时的微小冲力，解决启动慢的问题

    void Update()
    {
        // 1. 监测按键输入
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetZAngle += 90f;
            ApplyStartBoost(Vector3.left); // 向左旋转时，给球一个向左的微力
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetZAngle -= 90f;
            ApplyStartBoost(Vector3.right); // 向右旋转时，给球一个向右的微力
        }

        // 2. 计算目标四元数
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZAngle);

        // 3. 核心修复逻辑：只要地图还在旋转中，就强制唤醒小球
        // 这样可以防止物理引擎因为 Lerp 初期旋转太慢而让小球进入“半睡眠”状态
        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            if (ballRigidbody != null)
            {
                ballRigidbody.WakeUp(); // 强制唤醒物理计算
            }
        }

        // 4. 执行平滑旋转
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
    }

    /// <summary>
    /// 在按键瞬间给小球一个极其微小的冲力，打破起步的迟钝感
    /// </summary>
    void ApplyStartBoost(Vector3 direction)
    {
        if (ballRigidbody != null)
        {
            ballRigidbody.WakeUp();
            
            // 使用 VelocityChange 模式：无视质量，直接提供瞬时速度增量
            // 这样即便地图刚开始转，小球也会立刻动起来
            ballRigidbody.AddForce(direction * startKickForce, ForceMode.VelocityChange);
        }
    }
}