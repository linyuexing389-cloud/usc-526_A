using UnityEngine;

public class MovingSpike : MonoBehaviour
{
    [Header("移动设置")]
    public Transform[] waypoints; // 把那两个路径点拖到这里
    public float speed = 2f;      // 移动速度

    private int targetIndex = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        // 获取当前目标点
        Transform target = waypoints[targetIndex];

        // 使用 MoveTowards 保证匀速移动
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 如果距离目标点非常近了，切换到下一个点
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            // 在 0 和 1 之间切换（针对你的直线往返逻辑）
            targetIndex = (targetIndex + 1) % waypoints.Length;
        }
    }
}