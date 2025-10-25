using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("Camera Collision (FPS Edition)")]
    [Tooltip("Transform �ͧ��� ���ͨش��ع���ͧ (�蹵��˹� PlayerHead)")]
    public Transform head;

    [Tooltip("���� offset �ҡ��� ���������ͧ��誹���ŵ���ͧ")]
    public float offsetDistance = 0.05f;

    [Tooltip("���������������繡�ᾧ/��觡մ��ҧ")]
    public LayerMask collisionMask;

    [Tooltip("Smooth �������͹��Ǣͧ���ͧ����ͪ�")]
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

        // ������ҡ���˹���� (��Ǣͧ Player)
        Vector3 targetPosition = head.position + head.forward * offsetDistance;

        // ��Ǩ�ͺ���Ẻ SphereCast ���͡ѹ��������ͧ��� Mesh
        if (Physics.SphereCast(head.position, 0.1f, head.forward, out RaycastHit hit, offsetDistance, collisionMask))
        {
            // ��Ҫ�����¡��ͧ�͡�ҹԴ˹���
            targetPosition = hit.point - head.forward * 0.02f;
        }

        // ��Ѻ���˹觡��ͧ
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smooth);

        // �������ع���������� (�Ӥѭ����Ѻ FPS)
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
