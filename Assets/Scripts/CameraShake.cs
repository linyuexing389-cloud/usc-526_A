using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    Vector3 originalPos;

    void Awake()
    {
        // 不要 DontDestroyOnLoad 整个 Main Camera —— 否则 MainMenu 的相机会跑到关卡里把视角搞乱。
        // 每个场景的 Main Camera 都有自己的 CameraShake，进入时直接接管 Instance。
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