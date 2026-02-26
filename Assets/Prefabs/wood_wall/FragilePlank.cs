using UnityEngine;

public class FragilePlank : MonoBehaviour
{
    [Header("破坏设置")]
    [SerializeField] private float requiredMass = 20f;

    [Header("隐藏内容")]
    // 在 Inspector 面板里把想要隐藏的物体（敌人、陷阱、道具）拖进去
    [SerializeField] private GameObject[] hiddenObjects;

    private void Start()
    {
        // 自动初始化：游戏开始时，确保所有指定的物体都是隐藏状态
        foreach (GameObject obj in hiddenObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;

        if (rb != null && rb.mass >= requiredMass)
        {
            BreakWall();
        }
    }

    private void BreakWall()
    {
        // 1. 展现隐藏的秘密
        foreach (GameObject obj in hiddenObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                
                // 进阶建议：可以在这里给新出现的物体加个小特效
                // Instantiate(revealEffect, obj.transform.position, Quaternion.identity);
            }
        }

        // 2. 销毁墙壁
        Destroy(gameObject);
        
        Debug.Log("墙壁已破坏，隐藏内容已显现！");
    }
}