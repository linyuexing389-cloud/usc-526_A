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

    private bool isHeavy = false;
    private bool isScaling = false;
    private Vector3 visualTargetScale;
    private Vector3 visualOriginalScale;

    void Start()
    {
        visualOriginalScale = visual.localScale;

        ApplyNormalPhysics();
        smallCollider.enabled = true;
        largeCollider.enabled = false;
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
}