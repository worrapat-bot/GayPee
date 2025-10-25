using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("Camera Collision (FPS Edition)")]
    [Tooltip("Transform ของหัว หรือจุดหมุนกล้อง (เช่นตำแหน่ง PlayerHead)")]
    public Transform head;

    [Tooltip("ระยะ offset จากหัว เพื่อให้กล้องไม่ชนโมเดลตัวเอง")]
    public float offsetDistance = 0.05f;

    [Tooltip("เลเยอร์ที่ถือว่าเป็นกำแพง/สิ่งกีดขวาง")]
    public LayerMask collisionMask;

    [Tooltip("Smooth การเคลื่อนไหวของกล้องเมื่อชน")]
    public float smooth = 20f;

    private Vector3 desiredPosition;

    void Start()
    {
        if (head == null)
            head = transform.parent;
    }

    void LateUpdate()
    {
        if (head == null) return;

        // เริ่มจากตำแหน่งหัว (หัวของ Player)
        Vector3 targetPosition = head.position + head.forward * offsetDistance;

        // ตรวจรอบตัวแบบ SphereCast เพื่อกันไม่ให้กล้องเข้า Mesh
        if (Physics.SphereCast(head.position, 0.1f, head.forward, out RaycastHit hit, offsetDistance, collisionMask))
        {
            // ถ้าชนให้ถอยกล้องออกมานิดหน่อย
            targetPosition = hit.point - head.forward * 0.02f;
        }

        // ปรับตำแหน่งกล้อง
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smooth);

        // ทำให้หมุนตามหัวเสมอ (สำคัญสำหรับ FPS)
        transform.rotation = head.rotation;
    }

    void OnDrawGizmosSelected()
    {
        if (head == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(head.position, 0.1f);
        Gizmos.DrawLine(head.position, head.position + head.forward * offsetDistance);
    }
}
