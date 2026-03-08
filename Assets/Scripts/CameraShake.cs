using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    Vector3 originalPos;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            Vector3 offset = Random.insideUnitSphere * intensity;
            transform.localPosition = originalPos + offset;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}