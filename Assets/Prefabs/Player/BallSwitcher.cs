using UnityEngine;

public class BallSwitcher : MonoBehaviour
{
    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    // 自动记录你在 Inspector 或场景里调好的那个大小
    private Vector3 originalScale;

    [Header("1-普通球设置")]
    public float normalMass = 10f;
    public Color normalColor = Color.white;
    public float normalDamping = 0.001f;

    [Header("2-重力球设置")]
    public float heavyMass = 50f;
    public Color heavyColor = Color.black;
    public float heavyDamping = 1f;
    [Tooltip("重力球是原始大小的几倍？")]
    public float heavyScaleMultiplier = 2.0f; 
    public float heavySpeedMultiplier = 2.0f; 

    private bool isHeavy = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        // 【关键点】记录你在编辑器里手动设定的那个 Scale
        originalScale = transform.localScale;
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // 初始设为普通球，直接用记录好的原始大小
        SetBallProperties(normalMass, normalColor, normalDamping, originalScale, false);
    }

    void Update()
    {
        // 只有在状态改变时才允许切换，防止反复按键导致速度无限倍增
        if (Input.GetKeyDown(KeyCode.Alpha1) && isHeavy) 
        {
            SetBallProperties(normalMass, normalColor, normalDamping, originalScale, false);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isHeavy) 
        {
            // 基于原始大小乘以 1.2
            SetBallProperties(heavyMass, heavyColor, heavyDamping, originalScale * heavyScaleMultiplier, true);
        }
    }

    void SetBallProperties(float mass, Color color, float damping, Vector3 targetScale, bool turnToHeavy)
    {
        rb.mass = mass;
        rb.linearDamping = damping;
        rb.angularDamping = damping * 0.1f;
        meshRenderer.material.color = color;

        // 这里的 targetScale 是基于你原本设置好的大小算的，不会再“溢出”了
        transform.localScale = targetScale;

        // 速度逻辑
        if (turnToHeavy)
            rb.linearVelocity *= heavySpeedMultiplier;
        else
            rb.linearVelocity /= heavySpeedMultiplier;

        isHeavy = turnToHeavy;

        Debug.Log($"状态切换！当前缩放: {transform.localScale.x}, 质量: {mass}");
    }
}