using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI 面板引用")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject mechanicsPanel; // 新增：机制说明面板

    [Header("场景设置")]
    public string gameSceneName = "cube_map"; 

    void Start()
    {
        // 初始化：只显示第一个界面，其他全部隐藏
        if (startPanel != null) startPanel.SetActive(true);
        if (storyPanel != null) storyPanel.SetActive(false);
        if (mechanicsPanel != null) mechanicsPanel.SetActive(false);
    }

    // --- 按钮事件 ---

    // 1. 开始页面 -> 故事页面 (绑定到 StartPanel 的 Continue 按钮)
    public void OnClickContinue()
    {
        SwitchPanel(startPanel, storyPanel);
        Debug.Log("进入故事界面");
    }

    // 2. 故事页面 -> 机制页面 (绑定到 StoryPanel 的 Next/Continue 按钮)
    public void OnClickNextToMechanics()
    {
        SwitchPanel(storyPanel, mechanicsPanel);
        Debug.Log("进入机制说明界面");
    }

    // 3. 机制页面 -> 游戏场景 (绑定到 MechanicsPanel 的 Start 按钮)
    public void OnClickStart()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("场景名称为空！");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    // 提取的通用切换逻辑，让代码更干净
    private void SwitchPanel(GameObject from, GameObject to)
    {
        if (from != null) from.SetActive(false);
        if (to != null) to.SetActive(true);
    }
}