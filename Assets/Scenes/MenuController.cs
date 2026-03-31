using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("UI 面板引用")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMeshProUGUI warningText;

    private Coroutine _warningCoroutine;

    void Start()
    {
        // [双重保险] 确保每次回到主菜单时，时间流逝都是正常的
        Time.timeScale = 1f;

        // 确保开始面板是激活状态
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
            if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
            _warningCoroutine = StartCoroutine(HideWarningAfterDelay(3f));
        }
        return false;
    }

    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }
}
