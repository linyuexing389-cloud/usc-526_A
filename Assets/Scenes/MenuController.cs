using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("UI 面板引用")]
    [SerializeField] private GameObject startPanel;

    void Start()
    {
        // [双重保险] 确保每次回到主菜单时，时间流逝都是正常的
        Time.timeScale = 1f;

        // 确保开始面板是激活状态
        if (startPanel != null) 
        {
            startPanel.SetActive(true);
        }
    }

    // 1. 进入 教程关卡
    public void OnClickLoadTutorial()
    {
        SceneManager.LoadScene("tutorial_level");
    }

    // 2. 进入 地图一
    public void OnClickLoadCubeMap()
    {
        SceneManager.LoadScene("cube_map");
    }

    // 3. 进入 地图二
    public void OnClickLoadCubeMap1()
    {
        SceneManager.LoadScene("cube_map 1");
    }

    // 4. 进入 新地图 (新增的代码)
    public void OnClickLoadCubeMap2()
    {
        // 注意：这里的字符串必须和你的新场景文件名一模一样
        SceneManager.LoadScene("cube_map 2"); 
    }
}