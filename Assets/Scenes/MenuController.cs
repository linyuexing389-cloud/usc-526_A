using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("UI 面板引用")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMeshProUGUI warningText;

    void Start()
    {
        Time.timeScale = 1f;

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    // 1. 进入 教程关卡
    public void OnClickLoadTutorial()
    {
        SceneManager.LoadScene("tutorial_level");
    }

    // 2. 进入 地图一
    public void OnClickLoadCubeMap()
    {
        if (!CheckTutorialCompleted()) return;
        SceneManager.LoadScene("cube_map");
    }

    // 3. 进入 地图二
    public void OnClickLoadCubeMap1()
    {
        if (!CheckTutorialCompleted()) return;
        SceneManager.LoadScene("cube_map 1");
    }

    // 4. 进入 新地图
    public void OnClickLoadCubeMap2()
    {
        if (!CheckTutorialCompleted()) return;
        SceneManager.LoadScene("cube_map 2");
    }

    private bool CheckTutorialCompleted()
    {
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1) return true;

        if (warningText != null)
        {
            warningText.text = "Please complete the tutorial first!";
            warningText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWarning));
            Invoke(nameof(HideWarning), 3f);
        }
        return false;
    }

    private void HideWarning()
    {
        if (warningText != null) warningText.gameObject.SetActive(false);
    }
}
