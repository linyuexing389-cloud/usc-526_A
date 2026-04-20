using System.Collections;
using UnityEngine;

/// <summary>
/// 背景音乐管理器：跨场景单例，循环播放 BGM。
/// - 其他音效触发时自动压低 BGM 音量（Ducking）
/// - 玩家死亡时停止 BGM，3 秒后从头重新播放
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("音量设置")]
    [Range(0f, 1f)] public float normalVolume = 0.5f;
    [Range(0f, 1f)] public float duckedVolume = 0.15f;
    public float fadeSpeed = 6f; // 音量恢复/下降的插值速度

    private AudioSource _source;
    private AudioSource _sfxSource; // 2D 音效通道，避免 3D 衰减听不见
    private AudioClip _bgmClip;
    private float _duckTimer = 0f;     // > 0 时处于 Duck 状态
    private bool _suspended = false;   // 死亡等待期间暂停 BGM
    private Coroutine _restartCo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[BGMManager]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<BGMManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgmClip = Resources.Load<AudioClip>("Audio/bgm");
        _source = gameObject.AddComponent<AudioSource>();
        _source.clip = _bgmClip;
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = normalVolume;
        _source.spatialBlend = 0f; // 2D
        _source.ignoreListenerPause = true; // 不受 Time.timeScale = 0 影响
        if (_bgmClip != null) _source.Play();

        // 专门播 SFX 的 2D 通道
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f;
        _sfxSource.volume = 1f;
        _sfxSource.ignoreListenerPause = true;
    }

    /// <summary>2D 播放音效，避免 PlayClipAtPoint 的 3D 距离衰减问题。</summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    private void Update()
    {
        // 使用真实时间，避免 Time.timeScale = 0 时卡住
        float dt = Time.unscaledDeltaTime;

        if (_suspended)
        {
            // 死亡等待期间：快速淡出到静音
            _source.volume = Mathf.MoveTowards(_source.volume, 0f, fadeSpeed * dt);
            return;
        }

        if (_duckTimer > 0f)
        {
            _duckTimer -= dt;
            _source.volume = Mathf.Lerp(_source.volume, duckedVolume, fadeSpeed * dt);
        }
        else
        {
            _source.volume = Mathf.Lerp(_source.volume, normalVolume, fadeSpeed * dt);
        }
    }

    /// <summary>
    /// 压低 BGM 音量，持续 duration 秒后恢复。
    /// 可多次调用叠加（取最大剩余时长）。
    /// </summary>
    public void Duck(float duration)
    {
        if (duration <= 0f) duration = 0.5f;
        if (duration > _duckTimer) _duckTimer = duration;
    }

    /// <summary>
    /// 玩家死亡时调用：立刻停止 BGM，delay 秒后从头播放。
    /// </summary>
    public void StopAndRestart(float delay)
    {
        if (_restartCo != null) StopCoroutine(_restartCo);
        _restartCo = StartCoroutine(StopAndRestartCo(delay));
    }

    private IEnumerator StopAndRestartCo(float delay)
    {
        _suspended = true;
        // 等淡出完成一小段再真正 Stop
        yield return new WaitForSecondsRealtime(0.3f);
        _source.Stop();

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay - 0.3f));

        _suspended = false;
        _duckTimer = 0f;
        _source.volume = 0f;
        if (_bgmClip != null)
        {
            _source.time = 0f;
            _source.Play();
        }
        _restartCo = null;
    }
}
