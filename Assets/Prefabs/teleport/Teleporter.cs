using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("传送设置")]
    [Tooltip("将传送的目标位置（一个空物体）拖到这里")]
    public Transform destination; 

    [Header("效果设置")]
    public bool resetVelocity = true; // 传送后是否重置速度（防止飞出去）

    private void OnTriggerEnter(Collider other)
    {
        // 1. 检查碰到的物体是否有 Rigidbody (确保它是你的小球)
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null && destination != null)
        {
            // 2. 执行位移 (修改 Position)
            // 注意：如果是 Rigidbody 控制的物体，直接改 transform.position 是最快的
            rb.transform.position = destination.position;

            // 3. 物理逻辑：防止小球带着原来的惯性直接飞出地图
            if (resetVelocity)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; // 连旋转也停下
            }

            Debug.Log($"已传送到: {destination.name}");
        }
    }
}