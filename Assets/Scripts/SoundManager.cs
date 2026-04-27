using UnityEngine;

/// <summary>
/// 轻量级音效播放工具类。
/// 首次调用时从 Resources/Audio/ 下加载并缓存 AudioClip。
/// 使用 AudioSource.PlayClipAtPoint 播放，这样即使调用者被 Destroy 音效也能完整播完。
/// </summary>
public static class SoundManager
{
    private static AudioClip _keyClip;
    private static AudioClip _healthTimeClip;
    private static AudioClip _woodBreakClip;
    private static AudioClip _winClip;
    private static AudioClip _deathClip;
    private static AudioClip _hurtClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Preload()
    {
        // 提前加载，避免首次触发时卡顿
        _keyClip        = Resources.Load<AudioClip>("Audio/1");
        _healthTimeClip = Resources.Load<AudioClip>("Audio/2");
        _woodBreakClip  = Resources.Load<AudioClip>("Audio/3");
        _winClip        = Resources.Load<AudioClip>("Audio/4");
        _deathClip      = Resources.Load<AudioClip>("Audio/5");
        _hurtClip       = Resources.Load<AudioClip>("Audio/hurt");
    }

    private static AudioClip Get(ref AudioClip cached, string path)
    {
        if (cached == null) cached = Resources.Load<AudioClip>(path);
        return cached;
    }

    private static void Play(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        if (BGMManager.Instance != null)
        {
            // 走 2D 共享通道，避免 3D 衰减导致听不见
            BGMManager.Instance.PlaySFX(clip, volume);
            BGMManager.Instance.Duck(clip.length + 0.3f);
        }
        else
        {
            // 兜底（BGMManager 还没初始化时）
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }

    public static void PlayKey(Vector3 position, float volume = 1f)
    {
        Play(Get(ref _keyClip, "Audio/1"), position, volume);
    }

    public static void PlayHealthTime(Vector3 position, float volume = 1f)
    {
        Play(Get(ref _healthTimeClip, "Audio/2"), position, volume);
    }

    public static void PlayWoodBreak(Vector3 position, float volume = 1f)
    {
        Play(Get(ref _woodBreakClip, "Audio/3"), position, volume);
    }

    public static void PlayWin(Vector3 position, float volume = 1f)
    {
        Play(Get(ref _winClip, "Audio/4"), position, volume);
    }

    public static void PlayDeath(Vector3 position, float volume = 1f)
    {
        Play(Get(ref _deathClip, "Audio/5"), position, volume);
    }

    public static void PlayHurt(Vector3 position, float volume = 1f)
    {
        Play(Get(ref _hurtClip, "Audio/hurt"), position, volume);
    }
}
