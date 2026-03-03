using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game bar chart for Death Cause Distribution. Works in WebGL.
/// Toggle with F2; also shown automatically when game ends (LOSE).
/// </summary>
public class DeathAnalyticsGraphUI : MonoBehaviour
{
    [Header("Display")]
    public KeyCode toggleKey = KeyCode.F2;
    public float barMaxHeight = 200f;
    public float barWidth = 80f;
    public Color barTimeout = new Color(0.9f, 0.6f, 0.2f);
    public Color barSpikes = new Color(0.9f, 0.25f, 0.2f);
    public Color barTrap = new Color(0.2f, 0.5f, 0.9f);

    private GameObject root;
    private Image panelImage;
    private Image barTimeoutImg, barSpikesImg, barTrapImg;
    private Text labelTitle, labelTimeout, labelSpikes, labelTrap;
    private Text countTimeout, countSpikes, countTrap;

    private void Start()
    {
        EnsureCanvas();
        if (root != null)
            root.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Show()
    {
        EnsureCanvas();
        if (root == null) return;
        RefreshBars();
        root.SetActive(true);
    }

    public void Hide()
    {
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

    /// <summary>Call from GameManager when game ends with LOSE so the graph shows in WebGL.</summary>
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
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        root = new GameObject("DeathAnalyticsPanel");
        root.transform.SetParent(canvasGo.transform, false);

        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 320);
        rect.anchoredPosition = Vector2.zero;

        panelImage = root.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        float y = 120f;
        float rowH = 70f;

        labelTitle = AddText(root, "Death Cause Distribution", 24, new Vector2(0, y));
        y -= rowH;

        labelTimeout = AddText(root, "Timeout", 16, new Vector2(-120, y));
        barTimeoutImg = AddBar(root, barTimeout, new Vector2(-20, y), out countTimeout);
        y -= rowH;

        labelSpikes = AddText(root, "Spikes (red ball)", 16, new Vector2(-120, y));
        barSpikesImg = AddBar(root, barSpikes, new Vector2(-20, y), out countSpikes);
        y -= rowH;

        labelTrap = AddText(root, "Trap (red wall)", 16, new Vector2(-120, y));
        barTrapImg = AddBar(root, barTrap, new Vector2(-20, y), out countTrap);

        var closeHint = AddText(root, "Press " + toggleKey + " to close", 12, new Vector2(0, -130));
        closeHint.color = new Color(0.7f, 0.7f, 0.7f);
    }

    private static Text AddText(GameObject parent, string content, int fontSize, Vector2 pos)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(280, 30);
        rect.anchoredPosition = pos;
        var t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = fontSize;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null) t.font = font;
        t.color = Color.white;
        return t;
    }

    private static Image AddBar(GameObject parent, Color c, Vector2 pos, out Text countText)
    {
        var barGo = new GameObject("Bar");
        barGo.transform.SetParent(parent.transform, false);
        var rect = barGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.sizeDelta = new Vector2(80, 24);
        rect.anchoredPosition = pos;
        var img = barGo.AddComponent<Image>();
        img.color = c;

        var countGo = new GameObject("Count");
        countGo.transform.SetParent(parent.transform, false);
        var countRect = countGo.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.5f, 0.5f);
        countRect.anchorMax = new Vector2(0.5f, 0.5f);
        countRect.anchoredPosition = pos + new Vector2(60, 0);
        countRect.sizeDelta = new Vector2(60, 24);
        countText = countGo.AddComponent<Text>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null) countText.font = font;
        countText.fontSize = 14;
        countText.color = Color.white;
        countText.text = "0";
        return img;
    }

    private void RefreshBars()
    {
        int timeout = 0, spikes = 0, trap = 0;
        if (DeathAnalyticsManager.Instance != null)
            DeathAnalyticsManager.Instance.GetDeathCauseCounts(out timeout, out spikes, out trap);

        if (countTimeout != null) countTimeout.text = timeout.ToString();
        if (countSpikes != null) countSpikes.text = spikes.ToString();
        if (countTrap != null) countTrap.text = trap.ToString();

        int max = Mathf.Max(1, Mathf.Max(timeout, Mathf.Max(spikes, trap)));
        float scale = barMaxHeight / max;

        if (barTimeoutImg != null)
            SetBarHeight(barTimeoutImg.rectTransform, timeout * scale);
        if (barSpikesImg != null)
            SetBarHeight(barSpikesImg.rectTransform, spikes * scale);
        if (barTrapImg != null)
            SetBarHeight(barTrapImg.rectTransform, trap * scale);
    }

    private static void SetBarHeight(RectTransform rect, float height)
    {
        var size = rect.sizeDelta;
        size.y = Mathf.Max(8f, height);
        rect.sizeDelta = size;
    }
}
