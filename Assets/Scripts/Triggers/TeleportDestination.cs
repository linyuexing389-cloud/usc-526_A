using UnityEngine;

/// <summary>
/// 纯编辑器可视化组件：在 Scene 视图中显示传送目标点的线框球体。
/// 游戏运行时无任何效果，便于在编辑器中拖拽定位。
/// </summary>
public class TeleportDestination : MonoBehaviour
{
    public Color gizmoColor = new Color(0f, 0.8f, 1f, 0.8f);
    public float gizmoRadius = 0.8f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        // 十字准线
        float arm = gizmoRadius * 1.4f;
        Gizmos.DrawLine(transform.position - Vector3.right * arm, transform.position + Vector3.right * arm);
        Gizmos.DrawLine(transform.position - Vector3.up    * arm, transform.position + Vector3.up    * arm);
        Gizmos.DrawLine(transform.position - Vector3.forward * arm, transform.position + Vector3.forward * arm);
    }
}
