using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI; // **สำคัญ: ต้อง Import Library นี้**

// สคริปต์นี้ต้องมี FieldOfView และ NavMeshAgent ติดอยู่ด้วย
[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))] // **บังคับให้มี NavMeshAgent**
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการเคลื่อนที่ (NavMeshAgent Speed)")]
    public float moveSpeed = 3.0f; 
    [Tooltip("เวลารวมที่ใช้ในการหมุนและรอเมื่อถึงจุด Patrol")]
    public float waitTime = 2.5f;   
    [Tooltip("ความเร็วในการหมุนเพื่อหันหน้าไปยังจุดถัดไป")]
    public float rotationSpeed = 5.0f; // ใช้สำหรับ Search Rotation เท่านั้น
    
    [Header("Chase Settings")]
    [Tooltip("ระยะห่างขั้นต่ำที่ศัตรูจะหยุดเดินเมื่อไล่ล่าผู้เล่น")]
    public float stoppingDistance = 2.0f; 
    
    [Header("Search Rotation Settings")]
    [Tooltip("มุมหมุนค้นหา (ซ้ายและขวาจากแกนกลาง) เช่น 45 องศา")]
    public float searchAngle = 45f; 
    [Tooltip("ความเร็วในการหมุนค้นหา (องศาต่อวินาที)")]
    public float searchRotationSpeed = 100f; 
    [Tooltip("ความเร็วในการหมุนค้นหา 360 องศาเมื่อถึงจุดสุดท้าย")]
    public float finalSearchSpeed = 150f; 
    
    [Header("Patrol Points")]
    public List<Transform> patrolPoints = new List<Transform>();
    
    // สถานะปัจจุบันของศัตรู
    public bool isDetectingPlayer { get; private set; }

    private int currentPointIndex;
    private FieldOfView fieldOfView;
    private NavMeshAgent agent; // **ตัวแปรสำหรับ NavMeshAgent**

    // สถานะสำหรับ AI ขั้นสูง
    private bool isSearching = false;
    private Quaternion originalPatrolRotation; 
    
    private Vector3? lastKnownPlayerPosition = null; 
    private bool isCheckingLastKnownPosition = false;

    void Start()
    {
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>(); // **ดึง NavMeshAgent Component**
        
        // ตั้งค่าความเร็วและระยะหยุดให้กับ Agent
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.1f; // Agent ควรหยุดใกล้จุดมากๆ ก่อนจะเข้า Coroutine

        if (patrolPoints.Count == 0)
        {
            Debug.LogError("กรุณาเพิ่มจุด Patrol Points อย่างน้อยหนึ่งจุดใน EnemyPatrol component!");
            enabled = false; 
            return;
        }

        currentPointIndex = 0;
        // กำหนดจุดหมายแรกและเริ่มต้นการหมุนตั้งต้น
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        originalPatrolRotation = transform.rotation; 
    }

    void Update()
    {
        // 1. ตรวจสอบสถานะการมองเห็น
        bool currentlyDetecting = fieldOfView.VisibleTarget != null;
        
        if (currentlyDetecting)
        {
            isDetectingPlayer = true;
            // A. พบผู้เล่น: บันทึกตำแหน่งล่าสุดและเข้าสู่โหมดไล่ล่า
            lastKnownPlayerPosition = fieldOfView.VisibleTarget.position;
            ChasePlayer();
            
        }
        else // ผู้เล่นหลุดจากสายตา
        {
            isDetectingPlayer = false;
            
            // B. หากผู้เล่นหลุดจากสายตา และมีตำแหน่งสุดท้ายที่รู้ (ยังไม่ได้ตรวจสอบ)
            if (lastKnownPlayerPosition.HasValue && !isCheckingLastKnownPosition)
            {
                // เข้าสู่โหมดตรวจสอบตำแหน่งสุดท้าย
                StartCoroutine(GoToLastKnownPosition(lastKnownPlayerPosition.Value));
                isCheckingLastKnownPosition = true; 
                
            }
            // C. โหมดปกติ: ลาดตระเวน (จะถูกข้ามหากกำลังอยู่ใน Coroutine)
            else if (!isCheckingLastKnownPosition && !isSearching)
            {
                PatrolMovement();
            }
        }
    }

    // หยุด Coroutine ทั้งหมดที่อาจกำลังทำงาน และหยุด Agent
    private void StopAllAISequences()
    {
        StopAllCoroutines();
        isSearching = false;
        isCheckingLastKnownPosition = false;
        // หยุด NavMeshAgent เพื่อให้หยุดเดินทันที
        if (agent.enabled && agent.hasPath) 
        {
            agent.isStopped = true;
        }
    }

    private void ChasePlayer()
    {
        StopAllCoroutines(); // หยุดการลาดตระเวน/ค้นหาเดิมก่อนเข้าโหมดไล่ล่า
        isSearching = false;
        isCheckingLastKnownPosition = false;
        
        Transform target = fieldOfView.VisibleTarget;
        
        // 1. ตั้งค่า Agent ให้วิ่งไล่ผู้เล่น
        if (agent.isStopped) 
        {
            agent.isStopped = false;
        }

        // ตั้งค่าระยะหยุด Agent ให้เป็นระยะหยุดที่เราต้องการ
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);
        
        // การหมุนขณะไล่ล่าให้ Agent จัดการเอง 
    }

    /// <summary>
    /// Coroutine สำหรับการเดินไปยังตำแหน่งที่เห็นผู้เล่นครั้งสุดท้าย
    /// </summary>
    private IEnumerator GoToLastKnownPosition(Vector3 targetPosition)
    {
        // 1. เริ่มเดินไปยังตำแหน่งสุดท้ายโดยใช้ NavMeshAgent
        agent.stoppingDistance = 0.1f; // ตั้งระยะหยุดให้เล็กมาก
        agent.isStopped = false;
        agent.SetDestination(targetPosition);
        
        // รอจนกว่า Agent จะถึงจุดหมาย
        while (agent.remainingDistance > agent.stoppingDistance || agent.pathPending) 
        {
            // ถ้าพบผู้เล่นระหว่างทาง ให้หยุดและกลับไป ChasePlayer ทันที
            if (isDetectingPlayer)
            {
                isCheckingLastKnownPosition = false;
                yield break; 
            }
            yield return null;
        }
        
        // 2. ถึงจุดเป้าหมาย: เริ่มการหมุนค้นหา 360 องศา
        agent.isStopped = true; // หยุดเดินเมื่อถึงจุด
        lastKnownPlayerPosition = null; // เคลียร์ตำแหน่งสุดท้าย (ถือว่าตรวจสอบแล้ว)

        // หมุนค้นหา 360 องศา (หันซ้าย 180 แล้วกลับขวา 180)
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, 180, 0);
        
        // หมุนไป 180 องศา
        yield return StartCoroutine(RotateToTarget(targetRotation, finalSearchSpeed));
        
        // หมุนอีก 180 องศา เพื่อกลับมาที่เดิม (รวมเป็น 360)
        yield return StartCoroutine(RotateToTarget(currentRotation, finalSearchSpeed)); 
        
        // 3. จบการค้นหา ณ จุดสุดท้าย และกลับสู่เส้นทางลาดตระเวน
        isCheckingLastKnownPosition = false;
        
        // ตั้งค่าจุด Patrol ถัดไปทันทีเพื่อให้ PatrolMovement ทำงานต่อใน Update
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }


    private void PatrolMovement()
    {
        // 1. ถ้า Agent ถึงจุดหมายปัจจุบันแล้ว (ใกล้พอ) และไม่ได้กำลังหมุนค้นหา
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance && !isSearching) 
        {
            if (agent.isStopped == false) 
            {
                agent.isStopped = true; // หยุด Agent ก่อนเริ่มค้นหา
            }
            
            // บันทึกการหมุนเดิมเมื่อถึงจุด
            originalPatrolRotation = transform.rotation;
            // เริ่ม Coroutine การหมุนค้นหา
            StartCoroutine(SearchRoutine());
        } 
        else if (isSearching)
        {
            // ถ้ากำลังค้นหาอยู่ ไม่ต้องทำอะไร ปล่อยให้ Coroutine จัดการ
            return;
        }
        else // 2. กำลังเดินไปยังจุด Patrol
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
            // NavMeshAgent ยังคงเดินไปยังจุดหมายที่ถูกตั้งไว้
        }
    }
    
    /// <summary>
    /// Coroutine ที่จัดการลำดับการหมุนค้นหาของจุด Patrol: ซ้าย -> ขวา -> ตรงกลาง -> รอ 3 วิ -> ไปจุดถัดไป
    /// </summary>
    private IEnumerator SearchRoutine()
    {
        isSearching = true;
        
        // 1. หมุนไปทางซ้าย (-45 องศา)
        Quaternion leftTarget = originalPatrolRotation * Quaternion.Euler(0, -searchAngle, 0);
        yield return StartCoroutine(RotateToTarget(leftTarget, searchRotationSpeed));
        
        yield return new WaitForSeconds(waitTime / 2); 
        
        // 2. หมุนไปทางขวา (+45 องศา)
        Quaternion rightTarget = originalPatrolRotation * Quaternion.Euler(0, searchAngle, 0);
        yield return StartCoroutine(RotateToTarget(rightTarget, searchRotationSpeed));
        
        yield return new WaitForSeconds(waitTime / 2); 
        
        // 3. หมุนกลับไปที่มุมมองเดิม (ตรงกลาง)
        yield return StartCoroutine(RotateToTarget(originalPatrolRotation, searchRotationSpeed));
        
        // 4. หยุดรอ 3 วินาทีตรงกลาง
        yield return new WaitForSeconds(3f);
        
        // จบการค้นหาและไปจุดถัดไป
        isSearching = false;
        GoToNextPoint();
        
        // ** ตั้งค่าจุดหมายใหม่ให้ Agent เพื่อเริ่มเดินทันทีหลังจบ Coroutine
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    /// <summary>
    /// Coroutine ตัวช่วยในการหมุนตัวละครอย่างนุ่มนวลไปยังเป้าหมาย
    /// </summary>
    private IEnumerator RotateToTarget(Quaternion targetRotation, float rotationRate)
    {
        // ใช้ 0.1f เป็นค่าความคลาดเคลื่อนที่ยอมรับได้ 
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationRate * Time.deltaTime 
            );
            yield return null; // รอเฟรมถัดไป
        }
        transform.rotation = targetRotation; // ตั้งค่าให้ตรงเป๊ะเมื่อ Coroutine จบ
    }
    
    // ฟังก์ชันนี้ไม่จำเป็นต้องใช้แล้ว แต่เก็บไว้ในกรณีที่ต้องหมุนด้วยมือ
    private void RotateTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0; 
        direction.Normalize();

        if (direction == Vector3.zero) return; 

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            speed * Time.deltaTime
        );
    }
    
    private void GoToNextPoint()
    {
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
    }
}
