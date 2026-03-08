using UnityEngine;

public class BallSwitcher : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody rb;
    public SphereCollider smallCollider;
    public SphereCollider largeCollider;
    public Transform visual;
    public MeshRenderer meshRenderer;

    [Header("Normal")]
    public float normalMass = 10f;
    public float normalDamping = 0.001f;
    public Color normalColor = Color.white;

    [Header("Heavy")]
    public float heavyMass = 50f;
    public float heavyDamping = 1f;
    public Color heavyColor = Color.black;
    public float heavySpeedMultiplier = 2f;

    [Header("Visual Scale")]
    public float heavyScaleMultiplier = 1.2f;
    public float scaleLerpSpeed = 6f;

    [Header("Collision Check")]
    public LayerMask obstacleLayer;


    [Header("Impact")]
    public float impactSpeedThreshold = 30f;
    public float shakeBaseIntensity = 0.08f;
    public float shakeDuration = 0.15f;

    bool isHeavy = false;
    bool isScaling = false;
    [Header("Bottom Check")]
    public float bottomRayLength = 0.6f;
    bool isGrounded;


    Vector3 visualTargetScale;
    Vector3 visualOriginalScale;
    Vector3 lastVelocity;

    void Start()
    {
        visualOriginalScale = visual.localScale;

        ApplyNormalPhysics();
        smallCollider.enabled = true;
        largeCollider.enabled = false;

    }

    void FixedUpdate()
    {
        lastVelocity = rb.linearVelocity;

        isGrounded = false;

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isHeavy)
                SwitchToNormal();
            else
                TrySwitchToHeavy();
        }

        HandleVisualScale();
    }
    bool IsBottomHit()
    {
        Vector3 gravityDir = Physics.gravity.normalized;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDir = gravityDir;   // 向重力方向射线

        return Physics.Raycast(rayOrigin, rayDir, bottomRayLength);
    }
    void TrySwitchToHeavy()
    {
        if (!CanUseLargeCollider())
            return;

        isHeavy = true;

        smallCollider.enabled = false;
        largeCollider.enabled = true;

        ApplyHeavyPhysics();

        visualTargetScale = visualOriginalScale * heavyScaleMultiplier;
        isScaling = true;
    }

    void SwitchToNormal()
    {
        isHeavy = false;

        largeCollider.enabled = false;
        smallCollider.enabled = true;

        ApplyNormalPhysics();

        visualTargetScale = visualOriginalScale;
        isScaling = true;

    }

    bool CanUseLargeCollider()
    {
        float radius = largeCollider.radius;
        Vector3 center = transform.TransformPoint(largeCollider.center);

        Collider[] hits = Physics.OverlapSphere(center, radius, obstacleLayer);

        foreach (var c in hits)
        {
            if (c.attachedRigidbody == rb)
                continue;

            return false;
        }

        return true;
    }

    void HandleVisualScale()
    {
        if (!isScaling) return;

        visual.localScale = Vector3.Lerp(
            visual.localScale,
            visualTargetScale,
            Time.deltaTime * scaleLerpSpeed
        );

        if (Vector3.Distance(visual.localScale, visualTargetScale) < 0.01f)
        {
            visual.localScale = visualTargetScale;
            isScaling = false;
        }
    }

    void ApplyHeavyPhysics()
    {
        rb.mass = heavyMass;
        rb.linearDamping = heavyDamping;
        rb.angularDamping = heavyDamping * 0.1f;
        meshRenderer.material.color = heavyColor;
        rb.linearVelocity *= heavySpeedMultiplier;
    }

    void ApplyNormalPhysics()
    {
        rb.mass = normalMass;
        rb.linearDamping = normalDamping;
        rb.angularDamping = normalDamping * 0.1f;
        meshRenderer.material.color = normalColor;
        rb.linearVelocity /= heavySpeedMultiplier;
    }

    // -----------------------------
    // 使用接触法线判断接地
    // -----------------------------
    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // 如果接触法线和“反重力方向”接近
            float dot = Vector3.Dot(
                contact.normal,
                -Physics.gravity.normalized
            );

            if (dot > 0.5f)   // 约 60° 以内视为支撑面
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isHeavy) return;
        Debug.Log(IsBottomHit());
        if (!IsBottomHit()) return;   // 关键过滤

        Vector3 gravityDir = Physics.gravity.normalized;
        float fallSpeed = Vector3.Dot(lastVelocity, gravityDir);

        if (fallSpeed > impactSpeedThreshold)
        {
            float t = Mathf.InverseLerp(
                impactSpeedThreshold,
                impactSpeedThreshold * 2f,
                fallSpeed
            );

            float intensity = t * shakeBaseIntensity;

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(intensity, shakeDuration);
        }
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 gravityDir = Physics.gravity.normalized;
        Vector3 rayOrigin = transform.position;
        Vector3 rayEnd = rayOrigin + gravityDir * bottomRayLength;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayEnd);
    }
}