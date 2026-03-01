using UnityEngine;

public class RockBall : MonoBehaviour
{
    // 在 Inspector 面板里，把你的 Canvas 里的 Text (或者那个 Panel) 拖到这里
    public GameObject finishTextUI;

    void Start()
    {
        // 游戏开始时，先把字藏起来
        if (finishTextUI != null)
        {
            finishTextUI.SetActive(false);
        }
    }

    // 只要勾选了 Is Trigger 的物体，进入时会自动触发这个函数
    private void OnTriggerEnter(Collider other)
    {
        // 判定撞到的是不是终点
        if (other.CompareTag("Finish"))
        {
            Debug.Log("进入终点区域！");

            // 显示字样
            if (finishTextUI != null)
            {
                finishTextUI.SetActive(true);
            }

            // (可选) 如果你希望球到了终点就别动了
            // GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}