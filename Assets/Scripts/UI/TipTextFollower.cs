using UnityEngine;

public class TipTextFollower : MonoBehaviour
{
    [System.Serializable]
    public struct TipPair
    {
        public Transform worldAnchor;
        public RectTransform uiText;
    }

    public TipPair[] tips;
    public Camera cam;

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || tips == null) return;

        for (int i = 0; i < tips.Length; i++)
        {
            if (tips[i].worldAnchor == null || tips[i].uiText == null) continue;
            Vector3 screenPos = cam.WorldToScreenPoint(tips[i].worldAnchor.position);
            tips[i].uiText.position = screenPos;
        }
    }
}
