using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Full death screen: LOSE header + death cause bar chart + Retry button.
/// Created programmatically so it works in both scenes without scene setup.
/// Toggle with F2 during play; shown automatically on LOSE.
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
    private bool _isVisible = false;

    private void Start()
    {
        EnsureCanvas();
        // 如果 Show() 在 Start() 之前被调用（动态创建时），保持可见状态
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
        RefreshBars();
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
        else { RefreshBars(); root.SetActive(true); }
    }

    public static void ShowOnGameOver()
    {
        var graph = FindObjectOfType<DeathAnalyticsGraphUI>();
        if (graph != null) graph.Show();
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

        // Root panel — centered, fixed size
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

        // ── LOSE header ──────────────────────────────────────────────
        AddText(root, "LOSE", 96, FontStyle.Bold, new Vector2(0, 260), new Vector2(520, 110), font)
            .color = new Color(0.95f, 0.25f, 0.25f);

        // ── Stats title ──────────────────────────────────────────────
        AddText(root, "Death Cause Distribution", 26, FontStyle.Normal, new Vector2(0, 175), new Vector2(460, 36), font)
            .color = new Color(0.85f, 0.85f, 0.85f);

        // ── Bar rows ─────────────────────────────────────────────────
        float rowY = 100f;
        float rowH = 70f;

        barTimeoutImg = AddBarRow(root, "Timeout",       barTimeout, rowY,            font, out countTimeout);
        barSpikesImg  = AddBarRow(root, "Spikes (ball)", barSpikes,  rowY - rowH,     font, out countSpikes);
        barTrapImg    = AddBarRow(root, "Trap (wall)",   barTrap,    rowY - rowH * 2, font, out countTrap);

        // ── Retry button ─────────────────────────────────────────────
        var btnGo = new GameObject("RetryButton");
        btnGo.transform.SetParent(root.transform, false);
        var btnRect = btnGo.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(240, 65);
        btnRect.anchoredPosition = new Vector2(0, -255);

        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f);
        btnGo.AddComponent<CanvasRenderer>();

        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.75f, 0.3f);
        colors.pressedColor     = new Color(0.15f, 0.45f, 0.15f);
        btn.colors = colors;
        btn.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null) GameManager.Instance.RetryGame();
        });

        var btnLabel = AddText(btnGo, "Retry", 32, FontStyle.Bold, Vector2.zero, new Vector2(240, 65), font);
        btnLabel.color = Color.white;
        btnLabel.alignment = TextAnchor.MiddleCenter;
    }

    // Adds a horizontal bar row: [label]  [====bar====]  [count]
    private Image AddBarRow(GameObject parent, string label, Color color, float y, Font font, out Text countText)
    {
        float labelX = -185f;
        float barX   = -40f;
        float countX = 130f;

        AddText(parent, label, 20, FontStyle.Normal, new Vector2(labelX, y), new Vector2(150, 32), font)
            .alignment = TextAnchor.MiddleRight;

        var barGo = new GameObject("Bar");
        barGo.transform.SetParent(parent.transform, false);
        var barRect = barGo.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.pivot = new Vector2(0f, 0.5f);  // grows right
        barRect.sizeDelta = new Vector2(8f, 30f);
        barRect.anchoredPosition = new Vector2(barX, y);
        var barImg = barGo.AddComponent<Image>();
        barImg.color = color;
        barGo.AddComponent<CanvasRenderer>();

        var cntGo = new GameObject("Count");
        cntGo.transform.SetParent(parent.transform, false);
        var cntRect = cntGo.AddComponent<RectTransform>();
        cntRect.anchorMin = new Vector2(0.5f, 0.5f);
        cntRect.anchorMax = new Vector2(0.5f, 0.5f);
        cntRect.sizeDelta = new Vector2(60, 32);
        cntRect.anchoredPosition = new Vector2(countX, y);
        countText = cntGo.AddComponent<Text>();
        countText.font = font;
        countText.fontSize = 20;
        countText.color = Color.white;
        countText.text = "0";
        countText.alignment = TextAnchor.MiddleLeft;

        return barImg;
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
