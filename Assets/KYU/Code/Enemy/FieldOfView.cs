using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// สคริปต์จำลอง Field of View (FOV) สำหรับศัตรู/AI
/// ใช้สำหรับการตรวจจับเป้าหมาย (Target) ภายในระยะและมุมที่กำหนด
/// </summary>
public class FieldOfView : MonoBehaviour // ***[แก้ไขแล้ว]***: ต้องสืบทอดจาก MonoBehaviour เท่านั้น
{
    [Header("FOV Settings")]
    [Tooltip("รัศมีการมองเห็น (ระยะไกลสุดที่มองเห็น)")]
    public float ViewRadius = 10f;

    [Range(0, 360)]
    [Tooltip("มุมมอง (Field of View angle)")]
    public float ViewAngle = 110f;

    [Tooltip("LayerMask ของเป้าหมายที่ต้องการตรวจจับ (เช่น Player)")]
    public LayerMask TargetMask;

    [Tooltip("LayerMask ของสิ่งกีดขวาง (เช่น Wall, Object)")]
    public LayerMask ObstacleMask;

    [Tooltip("เวลาหน่วงในการตรวจจับ (เพื่อลดภาระ CPU)")]
    public float DelayCheckTime = 0.2f;

    // เป้าหมายที่ตรวจจับได้ (ตัวละครหลัก)
    [HideInInspector]
    public Transform VisibleTarget;


    // --- UNITY LIFECYCLE ---

    public void Start()
    {
        // เริ่ม Coroutine เพื่อตรวจสอบเป้าหมายเป็นระยะ
        StartCoroutine(FindTargetsWithDelay(DelayCheckTime));
    }


    // --- COROUTINE ---

    // Coroutine ที่ทำงานซ้ำเพื่อตรวจสอบเป้าหมายตามช่วงเวลาที่กำหนด
    public IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTarget();
        }
    }


    // --- CORE FOV LOGIC ---

    // # FindVisibleTarget(): void - ตรวจสอบว่าเป้าหมายอยู่ในระยะการมองเห็นหรือไม่
    public void FindVisibleTarget()
    {
        VisibleTarget = null; // ตั้งค่าเป้าหมายที่มองเห็นเป็น Null ทุกครั้งที่ตรวจสอบใหม่

        // 1. ตรวจสอบเป้าหมายทั้งหมดที่อยู่ในรัศมี (ViewRadius)
        // Physics.OverlapSphere จะคืนค่า Collider ทั้งหมดที่ซ้อนทับทรงกลม
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ViewRadius, TargetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            
            // 2. คำนวณทิศทางไปยังเป้าหมาย
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // 3. ตรวจสอบมุมมอง (ViewAngle)
            // Vector3.Angle คืนค่ามุมระหว่างทิศทางด้านหน้าของศัตรู (transform.forward) และทิศทางไปยังเป้าหมาย (dirToTarget)
            if (Vector3.Angle(transform.forward, dirToTarget) < ViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                
                // 4. ตรวจสอบสิ่งกีดขวาง (Obstacles) ด้วย Raycast
                // Raycast จากตำแหน่งศัตรูไปยังเป้าหมาย
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, ObstacleMask))
                {
                    // ถ้า Raycast ไม่ชนสิ่งกีดขวาง แสดงว่าเป้าหมายถูกมองเห็น
                    VisibleTarget = target;
                    // Debug.Log($"Target {target.name} ถูกพบแล้ว!"); 
                    return; // พบเป้าหมายแล้ว ให้หยุดการตรวจสอบทันที
                }
            }
        }
    }


    // --- HELPER METHODS ---

    // + DirFromAngle(float angleInDegrees, bool angleIsGlobal): Vector3
    // คำนวณทิศทางเวกเตอร์จากมุมที่กำหนด (สำหรับใช้ใน Gizmos)
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            // แปลงมุมจาก Local Space ให้เป็น Global Space โดยอิงจากมุมหมุนของศัตรู
            angleInDegrees += transform.eulerAngles.y;
        }
        
        // แปลงมุมเป็น Vector3
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }


    // --- DEBUG VISUALIZATION (Gizmos) ---

    // # OnDrawGizmos(): void - วาดเส้น FOV ใน Scene View เพื่อการ Debug
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        // 1. วาดรัศมีการมองเห็น (ViewRadius)
        Gizmos.DrawWireSphere(transform.position, ViewRadius);

        // 2. คำนวณและวาดเส้นขอบมุมมอง
        Vector3 viewAngleA = DirFromAngle(-ViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(ViewAngle / 2, false);

        // วาดเส้นรัศมีซ้าย-ขวา
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * ViewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * ViewRadius);

        // 3. ถ้ามองเห็นเป้าหมาย ให้วาดเส้นสีแดงไปยังเป้าหมาย
        if (VisibleTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, VisibleTarget.position);
        }
    }
}
