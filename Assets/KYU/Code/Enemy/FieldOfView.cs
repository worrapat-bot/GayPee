using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour
{
    [Header("View Settings")]
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public float detectionDelay = 0.2f;

    [Header("Layer Masks")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    // 💡 Property นี้คือที่ EnemyPatrol ใช้
    public Transform VisibleTarget { get; private set; }

    private List<Transform> visibleTargetsList = new List<Transform>();
    private Collider[] targetsInViewRadius;

    void Start()
    {
        StartCoroutine(FindTargetsWithDelay(detectionDelay));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        VisibleTarget = null; // รีเซ็ตเป้าหมายทุกครั้งที่เช็ค
        visibleTargetsList.Clear();
        
        targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // 1. เช็คว่าอยู่ในองศาการมองเห็นหรือไม่
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                // 2. เช็คว่ามีอะไรขวางกั้นระหว่างศัตรูกับเป้าหมายหรือไม่
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    // ถ้าไม่เจออะไรขวาง = มองเห็น
                    visibleTargetsList.Add(target);
                }
            }
        }

        // 💡 ตั้งค่าเป้าหมายที่มองเห็น (ถ้าเห็นหลายตัว เอาตัวแรกที่เจอ)
        if (visibleTargetsList.Count > 0)
        {
            VisibleTarget = visibleTargetsList[0];
        }
    }

    // --- (Optional) ใช้วาด Gizmos ใน Editor เพื่อดูรัศมีสายตา ---
    
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        if (VisibleTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, VisibleTarget.position);
        }
    }
}