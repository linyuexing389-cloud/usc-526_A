using UnityEngine;

public class PickupPopupText : MonoBehaviour
{
    private const float Lifetime = 1.1f;
    private const float RiseDistance = 1.0f;

    private TextMesh textMesh;
    private MeshRenderer meshRenderer;
    private Vector3 startPosition;
    private Color baseColor;
    private float elapsed;

    public static void Show(Vector3 worldPosition, string message, Color color)
    {
        var go = new GameObject("PickupPopupText");
        go.transform.position = worldPosition + Vector3.up * 0.6f;

        var popup = go.AddComponent<PickupPopupText>();
        popup.Initialize(message, color);
    }

    private void Initialize(string message, Color color)
    {
        textMesh = gameObject.AddComponent<TextMesh>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        textMesh.text = message;
        textMesh.fontSize = 96;
        textMesh.characterSize = 0.1125f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 500;
        }

        startPosition = transform.position;
        baseColor = color;
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / Lifetime);

        transform.position = startPosition + Vector3.up * (RiseDistance * t);

        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }

        if (textMesh != null)
        {
            Color faded = baseColor;
            faded.a = 1f - t;
            textMesh.color = faded;
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        if (elapsed >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
