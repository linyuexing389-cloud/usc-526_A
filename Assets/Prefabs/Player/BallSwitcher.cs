using UnityEngine;

public class BallSwitcher : MonoBehaviour
{
    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    // 自动记录你在 Inspector 里初始调好的那个大小
    private Vector3 originalScale;

    [Header("1-普通球设置")]
    public float normalMass = 10f;
    public Color normalColor = Color.white;
    public float normalDamping = 0.001f;

    [Header("2-重力球设置")]
    public float heavyMass = 50f;
    public Color heavyColor = Color.black;
    public float heavyDamping = 1f;

    [Tooltip("重力球是原始大小的几倍？已按要求设为 1.5")]
    public float heavyScaleMultiplier = 1.2f; 
    
    public float heavySpeedMultiplier = 2.0f; 

    private bool isHeavy = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        // 【核心】在游戏启动瞬间，记住你手动设置的那个 Scale
        originalScale = transform.localScale;
        
        // 建议开启连续碰撞检测，防止重力球速度太快穿模
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // 初始化状态：普通球
        SetBallProperties(normalMass, normalColor, normalDamping, originalScale, false);
    }

    void Update()
    {
        // 状态切换逻辑
        if (Input.GetKeyDown(KeyCode.Alpha1) && isHeavy) 
        {
            SetBallProperties(normalMass, normalColor, normalDamping, originalScale, false);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isHeavy) 
        {
            // 基于 Start 记录的原始大小乘以 1.5
            Vector3 targetHeavyScale = originalScale * heavyScaleMultiplier;
            SetBallProperties(heavyMass, heavyColor, heavyDamping, targetHeavyScale, true);
        }
    }

    void SetBallProperties(float mass, Color color, float damping, Vector3 targetScale, bool turnToHeavy)
    {
        rb.mass = mass;
        
        // 注意：如果你使用的是 Unity 6 或更高版本，使用的是 linearDamping
        // 如果是旧版本，请改回 rb.drag = damping;
        rb.linearDamping = damping;
        rb.angularDamping = damping * 0.1f;
        
        meshRenderer.material.color = color;

        // 瞬间切换大小
        transform.localScale = targetScale;

        // 速度补偿逻辑
        if (turnToHeavy)
            rb.linearVelocity *= heavySpeedMultiplier;
        else
            rb.linearVelocity /= heavySpeedMultiplier;

        isHeavy = turnToHeavy;

        Debug.Log($"[状态切换] 重力模式: {isHeavy} | 当前缩放: {transform.localScale.x} (原始的 {heavyScaleMultiplier} 倍)");
    }
}