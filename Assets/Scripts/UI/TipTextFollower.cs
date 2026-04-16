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
}
