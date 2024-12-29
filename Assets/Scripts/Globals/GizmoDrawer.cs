using UnityEngine;

public class GizmoDrawer : MonoBehaviour
{
    public Color gizmoColor = Color.black;
    public float gizmoSize = 0.1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}