using UnityEngine;

public class FragilePlank : MonoBehaviour
{
    [Header("Destruction Settings (破坏设置)")]
    [Tooltip("只有碰撞物体的质量大于此值，墙才会碎 (普通球10, 重力球50)")]
    [SerializeField] private float requiredMass = 20f;

    [Header("Hidden Content to Reveal (激活隐藏物体)")]
    [Tooltip("撞开后，这些物体会从隐藏变为激活状态 (如：隐藏的钥匙、出口)")]
    [SerializeField] private GameObject[] hiddenObjects;

    [Header("Linked Objects to Remove (联动彻底销毁)")]
    [Tooltip("撞开后，这些物体会彻底从场景中删除 (如：远端挡路的空气墙、装饰墙)")]
    [SerializeField] private GameObject[] objectsToDestroy;

    private void Start()
    {
        // 游戏开始时，确保所有【隐藏内容】都是关闭状态
        foreach (GameObject obj in hiddenObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        // 注意：objectsToDestroy 不需要在这里隐藏，因为它们本身就是挡路的，撞开才消失
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 获取碰撞物体的 Rigidbody（小球）
        Rigidbody rb = collision.rigidbody;

        // 检查质量是否达标
        if (rb != null && rb.mass >= requiredMass)
        {
            HandleBreakage();
        }
    }

    private void HandleBreakage()
    {
        // 1. 激活原本隐藏的内容 (SetActive)
        if (hiddenObjects != null)
        {
            foreach (GameObject obj in hiddenObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"[Hidden Content] {obj.name} is now active.");
                }
            }
        }

        // 2. 联动销毁指定的远端墙体 (Destroy)
        if (objectsToDestroy != null)
        {
            foreach (GameObject target in objectsToDestroy)
            {
                if (target != null)
                {
                    Destroy(target);
                    Debug.Log($"[Linked Destruction] {target.name} has been erased.");
                }
            }
        }

        // 3. 销毁这面触发墙本身
        Destroy(gameObject);
        
        Debug.Log("<color=yellow>Fragile Plank Broken!</color> Secrets revealed and linked walls removed.");
    }
}