using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Full death/win screen.
/// Created programmatically so it works in both scenes without scene setup.
/// Toggle with F2 during play; shown automatically on WIN/LOSE.
/// Shows death cause bar chart ONLY on LOSE.
/// </summary>
public class DeathAnalyticsGraphUI : MonoBehaviour
{
    [Header("Display")]
    public KeyCode toggleKey = KeyCode.F2;
    public float barMaxWidth = 200f;
    public Color barTimeout = new Color(0.9f, 0.6f, 0.2f);
    public Color barSpikes = new Color(0.9f, 0.25f, 0.2f);
    public Color barTrap = new Color(0.2f, 0.5f, 0.9f);

    private GameObject root;
    private Image barTimeoutImg, barSpikesImg, barTrapImg;
    private Text countTimeout, countSpikes, countTrap;
    private Text _headerText;
    private bool _isVisible = false;

    // ─── [UI 引用对象] ───────────────────────────────────────────
    private GameObject _statsTitleGo;
    private GameObject _timeoutRowGo, _spikesRowGo, _trapRowGo;
    
    // 新增：两个按钮的引用
    private GameObject _retryBtnGo;
    private GameObject _menuBtnGo;
    private Button _retryBtn;
    private Button _menuBtn;
    private RectTransform _menuBtnRect; // 用于在 WIN 时将菜单按钮居中
    // ────────────────────────────────────────────────────────────

    private void Start()
    {
        EnsureCanvas();
        if (root != null && !_isVisible)
            root.SetActive(false);
    }

    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Hide();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Show()
    {
        _isVisible = true;
        EnsureCanvas();
        if (root == null) return;
        
        if (_statsTitleGo != null && _statsTitleGo.activeSelf)
        {
            RefreshBars();
        }
        root.SetActive(true);
    }

    public void Hide()
    {
        _isVisible = false;
        if (root != null)
            root.SetActive(false);
    }

    public void Toggle()
    {
        if (root == null) EnsureCanvas();
        if (root == null) return;
        if (root.activeSelf) Hide();
        else { Show(); }
    }

    public static void ShowOnGameOver(bool isWin = false)
    {
        var graph = FindAnyObjectByType<DeathAnalyticsGraphUI>();
        
        if (graph == null) 
        {
            var managerGo = GameObject.Find("DeathAnalyticsManager");
            if (managerGo == null)
            {
                managerGo = new GameObject("DeathAnalyticsManager");
                managerGo.AddComponent<DeathAnalyticsManager>(); 
            }
            graph = managerGo.AddComponent<DeathAnalyticsGraphUI>();
        }

        graph.ApplyState(isWin);
        graph.Show();
    }

    private void ApplyState(bool isWin)
    {
        EnsureCanvas(); 

        if (_headerText != null)
        {
            _headerText.text  = isWin ? "WIN" : "LOSE";
            _headerText.color = isWin
                ? new Color(1f, 0.82f, 0.1f)   // 金色
                : new Color(0.95f, 0.25f, 0.25f); // 红色
        }

        // ─── [按钮排版与事件绑定逻辑] ─────────────────────────────────
        // LOSE时显示Retry按钮，WIN时隐藏
        if (_retryBtnGo != null) 
            _retryBtnGo.SetActive(!isWin);

        // Main Menu 按钮永远显示，但位置根据胜负状态变化
        if (_menuBtnRect != null)
        {
            // WIN时居中 (X=0)，LOSE时偏右显示 (X=150) 与偏左的Retry并排
            _menuBtnRect.anchoredPosition = isWin ? new Vector2(0, -255) : new Vector2(150, -255);
        }

        // 绑定重试事件
        if (_retryBtn != null)
        {
            _retryBtn.onClick.RemoveAllListeners();
            _retryBtn.onClick.AddListener(() => { 
                if (GameManager.Instance != null) GameManager.Instance.RetryGame(); 
            });
        }

        // 绑定返回主菜单事件
        if (_menuBtn != null)
        {
            _menuBtn.onClick.RemoveAllListeners();
            _menuBtn.onClick.AddListener(() => { 
                Time.timeScale = 1; 
                SceneManager.LoadScene("Scene_MainMenu"); 
            });
        }
        // ────────────────────────────────────────────────────────────

        // 统计图的显示/隐藏逻辑
        bool showStats = !isWin;

        if (_statsTitleGo != null) _statsTitleGo.SetActive(showStats);
        if (_timeoutRowGo != null)  _timeoutRowGo.SetActive(showStats);
        if (_spikesRowGo != null)   _spikesRowGo.SetActive(showStats);
        if (_trapRowGo != null)     _trapRowGo.SetActive(showStats);
    }

    private void EnsureCanvas()
    {
        if (root != null) return;

        var canvasGo = new GameObject("DeathAnalyticsCanvas");
        canvasGo.transform.SetParent(transform);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        root = new GameObject("DeathScreen");
        root.transform.SetParent(canvasGo.transform, false);
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(840, 680);
        rootRect.anchoredPosition = Vector2.zero;
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.97f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Header
        _headerText = AddText(root, "LOSE", 96, FontStyle.Bold, new Vector2(0, 260), new Vector2(520, 110), font);
        
        // Stats
        var statsTitleText = AddText(root, "Death Cause Distribution", 26, FontStyle.Normal, new Vector2(0, 175), new Vector2(460, 36), font);
        statsTitleText.color = new Color(0.85f, 0.85f, 0.85f);
        _statsTitleGo = statsTitleText.gameObject;

        float rowY = 100f;
        float rowH = 70f;
        _timeoutRowGo = AddBarRow(root, "Timeout",       barTimeout, rowY,            font, out barTimeoutImg, out countTimeout);
        _spikesRowGo  = AddBarRow(root, "Spikes (ball)", barSpikes,  rowY - rowH,     font, out barSpikesImg,  out countSpikes);
        _trapRowGo    = AddBarRow(root, "Trap (wall)",   barTrap,    rowY - rowH * 2, font, out barTrapImg,    out countTrap);

        // ─── [创建两个按钮] ──────────────────────────────────────────
        // 1. 创建 Retry 按钮 (绿色，偏左放置，X = -150)
        _retryBtnGo = CreateButton(root, "RetryButton", "Retry", new Color(0.2f, 0.6f, 0.2f), new Vector2(-150, -255), font, out _retryBtn);

        // 2. 创建 Main Menu 按钮 (深蓝灰，偏右放置，X = 150)
        _menuBtnGo = CreateButton(root, "MenuButton", "Main Menu", new Color(0.15f, 0.15f, 0.4f), new Vector2(150, -255), font, out _menuBtn);
        _menuBtnRect = _menuBtnGo.GetComponent<RectTransform>(); // 保存 RectTransform 以便动态调整位置
        // ────────────────────────────────────────────────────────────
    }

    // 提取出一个辅助方法来快速生成标准按钮，减少重复代码
    private GameObject CreateButton(GameObject parent, string btnName, string labelText, Color bgColor, Vector2 anchoredPos, Font font, out Button btn)
    {
        var btnGo = new GameObject(btnName);
        btnGo.transform.SetParent(parent.transform, false);
        var rect = btnGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(240, 65);
        rect.anchoredPosition = anchoredPos;

        var img = btnGo.AddComponent<Image>();
        img.color = bgColor;
        btnGo.AddComponent<CanvasRenderer>();

        btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        // 自动计算一下按钮的悬停和按下高亮颜色
        colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f);
        colors.pressedColor     = new Color(bgColor.r - 0.1f,  bgColor.g - 0.1f,  bgColor.b - 0.1f);
        btn.colors = colors;

        var label = AddText(btnGo, labelText, 32, FontStyle.Bold, Vector2.zero, new Vector2(240, 65), font);
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        return btnGo;
    }

    private GameObject AddBarRow(GameObject parent, string label, Color color, float y, Font font, out Image barImg, out Text countText)
    {
        var rowGo = new GameObject("Row_" + label);
        rowGo.transform.SetParent(parent.transform, false);
        var rect = rowGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(800, 40); 
        rect.anchoredPosition = new Vector2(0, y);

        float labelX = -185f;
        float barX   = -40f;
        float countX = 130f;

        AddText(rowGo, label, 20, FontStyle.Normal, new Vector2(labelX, 0), new Vector2(150, 32), font)
            .alignment = TextAnchor.MiddleRight;

        var barGo = new GameObject("BarVisual");
        barGo.transform.SetParent(rowGo.transform, false);
        var barRect = barGo.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.pivot = new Vector2(0f, 0.5f);  
        barRect.sizeDelta = new Vector2(8f, 30f);
        barRect.anchoredPosition = new Vector2(barX, 0);
        barImg = barGo.AddComponent<Image>();
        barImg.color = color;
        barGo.AddComponent<CanvasRenderer>();

        var cntGo = new GameObject("Count");
        cntGo.transform.SetParent(rowGo.transform, false);
        var cntRect = cntGo.AddComponent<RectTransform>();
        cntRect.anchorMin = new Vector2(0.5f, 0.5f);
        cntRect.anchorMax = new Vector2(0.5f, 0.5f);
        cntRect.sizeDelta = new Vector2(60, 32);
        cntRect.anchoredPosition = new Vector2(countX, 0);
        countText = cntGo.AddComponent<Text>();
        countText.font = font;
        countText.fontSize = 20;
        countText.color = Color.white;
        countText.text = "0";
        countText.alignment = TextAnchor.MiddleLeft;

        return rowGo; 
    }

    private static Text AddText(GameObject parent, string content, int fontSize, FontStyle style,
                                Vector2 pos, Vector2 size, Font font)
    {
        var go = new GameObject("Text_" + content);
        go.transform.SetParent(parent.transform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        go.AddComponent<CanvasRenderer>();
        var t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = fontSize;
        t.fontStyle = style;
        if (font != null) t.font = font;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        return t;
    }

    private void RefreshBars()
    {
        int timeout = 0, spikes = 0, trap = 0;
        if (DeathAnalyticsManager.Instance != null)
            DeathAnalyticsManager.Instance.GetDeathCauseCounts(out timeout, out spikes, out trap);

        SetBarWidth(barTimeoutImg, timeout);
        SetBarWidth(barSpikesImg, spikes);
        SetBarWidth(barTrapImg, trap);

        if (countTimeout != null) countTimeout.text = timeout.ToString();
        if (countSpikes  != null) countSpikes.text  = spikes.ToString();
        if (countTrap    != null) countTrap.text     = trap.ToString();
    }

    private void SetBarWidth(Image bar, int count)
    {
        if (bar == null) return;
        int timeout = 0, spikes = 0, trap = 0;
        if (DeathAnalyticsManager.Instance != null)
            DeathAnalyticsManager.Instance.GetDeathCauseCounts(out timeout, out spikes, out trap);
        int max = Mathf.Max(1, timeout, spikes, trap);
        var sz = bar.rectTransform.sizeDelta;
        sz.x = Mathf.Max(8f, (float)count / max * barMaxWidth);
        bar.rectTransform.sizeDelta = sz;
    }
}