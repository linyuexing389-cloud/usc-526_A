using UnityEngine;

public class TipZone : MonoBehaviour
{
    public RectTransform uiText;

    private Camera cam;

    void Start()
    {
        if (uiText != null)
            uiText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (uiText != null && uiText.gameObject.activeSelf)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            uiText.position = cam.WorldToScreenPoint(transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && uiText != null)
            uiText.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && uiText != null)
            uiText.gameObject.SetActive(false);
    }
}
