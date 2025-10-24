using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

// สคริปต์นี้ต้องมี FieldOfView, NavMeshAgent และ Animator ติดอยู่ด้วย
[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] 
public class EnemyPatrol : MonoBehaviour
{
    // เปลี่ยนเป็น protected fields เพื่อให้คลาสลูกเข้าถึงได้
    [field: Header("Movement Settings")]
    [Tooltip("ความเร็วในการเดินลาดตระเวน (Base Patrol Speed)")]
    [field: SerializeField] protected float moveSpeed { get; set; } = 3.0f; // *ปรับค่าพื้นฐานกลับเป็น 3.0f สำหรับ Patrol*
    [Tooltip("ความเร็วเมื่อไล่ล่าผู้เล่น (Chase Speed)")]
    [field: SerializeField] protected float chaseSpeed { get; set; } = 7.0f; // **ค่าใหม่: 7.0f**
    [field: SerializeField] protected float waitTime { get; set; } = 2.5f; 
    [field: SerializeField] protected float rotationSpeed { get; set; } = 5.0f; 

    [field: Header("Chase Settings")]
    [Tooltip("ระยะห่างขั้นต่ำที่ศัตรูจะหยุดเดินเมื่อไล่ล่าผู้เล่น")]
    [field: SerializeField] protected float stoppingDistance { get; set; } = 2.0f; 
    
    // **ตัวแปรใหม่: Thresholds สำหรับควบคุมแอนิเมชัน Run**
    [Header("Animation Settings")]
    [Tooltip("ความเร็วสูงสุดในการเดิน ก่อนเปลี่ยนเป็นวิ่ง (Walk Speed Threshold)")]
    [field: SerializeField] protected float walkSpeedThreshold = 3.0f;
    [field: SerializeField] protected string jumpscareTriggerName = "DoJumpscare";
    
    [Header("Search Settings")]
    [Tooltip("มุมหมุนค้นหา (ซ้ายและขวาจากแกนกลาง)")]
    [field: SerializeField] protected float searchAngle { get; set; } = 45f;
    [field: SerializeField] protected float searchRotationSpeed { get; set; } = 100f;

    [field: Header("Patrol Points")]
    // เปลี่ยนเป็น protected List เพื่อให้คลาสลูกจัดการจุดลาดตระเวนได้
    [field: SerializeField] protected List<Transform> patrolPoints { get; set; } = new List<Transform>();

    public bool isDetectingPlayer { get; protected set; } 

    protected int currentPointIndex;
    protected FieldOfView fieldOfView;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected bool isSearching = false;
    protected bool isCheckingLastKnownPosition = false;
    protected Vector3? lastKnownPlayerPosition = null;
    protected Quaternion originalRotation;
    // ** ลบ jumpScareTriggered ออก (ไม่จำเป็นเมื่อใช้ Bool) **


    // เปลี่ยน Start เป็น protected virtual
    protected virtual void Start()
    {
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // ตั้งค่าเริ่มต้น (Patrol Speed)
        agent.speed = moveSpeed; 
        
        agent.stoppingDistance = 0.1f; 

        if (patrolPoints.Count == 0)
        {
            Debug.LogWarning("EnemyPatrol: Patrol Points list is empty. AI will remain idle unless chased.");
        }

        currentPointIndex = 0;
        if (patrolPoints.Count > 0)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    // เปลี่ยน Update เป็น protected virtual
    protected virtual void Update()
    {
        // ** Jumpscare Lock: ใช้ IsHit Lock **
        if (animator.GetBool("IsHit")) return; // ถ้า IsHit เป็น true ให้หยุด AI
        
        // --- อัปเดต Animation Speed Logic (3 ระดับ) ---
        UpdateAnimationSpeed();
        // --- End Animation Speed Logic ---

        bool currentlyDetecting = fieldOfView.VisibleTarget != null;

        if (currentlyDetecting) 
        {
            isDetectingPlayer = true;
            lastKnownPlayerPosition = fieldOfView.VisibleTarget.position;
            ChasePlayer();
        }
        else
        {
            isDetectingPlayer = false;

            if (lastKnownPlayerPosition.HasValue && !isCheckingLastKnownPosition)
            {
                StartCoroutine(GoToLastKnownPosition(lastKnownPlayerPosition.Value));
                isCheckingLastKnownPosition = true;
            }
            else if (!isCheckingLastKnownPosition && !isSearching)
            {
                PatrolMovement();
            }
        }
    }
    
    // ** ฟังก์ชันใหม่: ตรวจจับการสัมผัส (Trigger Collision) **
    protected void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าชนกับผู้เล่น และ Jumpscare ยังไม่ถูกเรียก
        if (other.CompareTag("Player") && !animator.GetBool("IsHit"))
        {
            HandleJumpscare();
        }
    }

    // ** เมธอดใหม่: จัดการ Jumpscare Animation **
    protected void HandleJumpscare()
    {
        // 1. ล็อกสถานะ Jumpscare ใน Animator
        animator.SetBool("IsHit", true);
        StopAllAISequences();
        
        // 2. สั่งเล่น Animation Jumpscare ทันที 
        if (animator != null && jumpscareTriggerName != "")
        {
            animator.SetTrigger(jumpscareTriggerName);
        }
        
        // TODO: เพิ่ม Logic การสั่นกล้อง/เสียง
        
        // ณ จุดนี้ มอนสเตอร์จะเล่น Jumpscare Animation และค้างอยู่
        // คุณสามารถเพิ่ม Coroutine ที่จะทำ Game Over หรือเปลี่ยน Scene ในภายหลัง
    }
    
    // ** เมธอดใหม่: ควบคุมแอนิเมชัน 3 ระดับ **
    protected void UpdateAnimationSpeed()
    {
        if (animator == null || agent == null) return;
        
        float currentVelocityMagnitude = (agent.enabled && !agent.isStopped) ? agent.velocity.magnitude : 0f;
        
        float speedValue;

        // 1. ตรวจสอบความเร็วจริง: ถ้าต่ำกว่า 0.1 ถือว่าหยุดนิ่ง (Idle)
        if (currentVelocityMagnitude < 0.1f)
        {
            speedValue = 0f;
        }
        // 2. ถ้าเร็วกว่า 0.1 แต่ไม่ถึง Threshold (Walk)
        else if (currentVelocityMagnitude <= walkSpeedThreshold)
        {
            speedValue = Mathf.InverseLerp(0f, walkSpeedThreshold, currentVelocityMagnitude);
        }
        // 3. ถ้าเร็วกว่า Threshold (Run)
        else // currentVelocityMagnitude > walkSpeedThreshold
        {
            float excessSpeed = currentVelocityMagnitude - walkSpeedThreshold; 
            speedValue = 1.0f + (excessSpeed * 0.5f); // 0.5f เป็น factor ปรับความไว Run
        }

        animator.SetFloat("Speed", speedValue, 0.1f, Time.deltaTime);
    }
    
    protected void StopAllAISequences()
    {
        StopAllCoroutines();
        isSearching = false;
        isCheckingLastKnownPosition = false;
        if (agent != null && agent.enabled)
            agent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    // เปลี่ยนเป็น protected virtual เพื่อให้คลาสลูก (เช่น Ghost) สามารถ Override การไล่ล่าได้
    protected virtual void ChasePlayer()
    {
        StopAllCoroutines();
        isSearching = false;
        isCheckingLastKnownPosition = false;

        Transform target = fieldOfView.VisibleTarget;
        
        // ** สำคัญ: เพิ่มความเร็วเป็น 7.0f **
        agent.speed = chaseSpeed; 

        if (agent.isStopped)
            agent.isStopped = false;

        agent.stoppingDistance = stoppingDistance; 
        agent.SetDestination(target.position);
    }
    
    protected IEnumerator GoToLastKnownPosition(Vector3 targetPosition)
    {
        // [Logic GoToLastKnownPosition unchanged for brevity]
        agent.stoppingDistance = 0.1f;
        agent.isStopped = false;
        agent.SetDestination(targetPosition);

        while (agent.remainingDistance > agent.stoppingDistance || agent.pathPending)
        {
            if (isDetectingPlayer)
            {
                isCheckingLastKnownPosition = false;
                yield break;
            }
            yield return null;
        }

        agent.isStopped = true;
        // ทำการค้นหา 360 องศาที่ตำแหน่งสุดท้าย
        yield return StartCoroutine(SearchRoutine());

        lastKnownPlayerPosition = null;
        isCheckingLastKnownPosition = false;

        GoToNextPoint();
        
        // ** สำคัญ: กลับมาใช้ Patrol Speed **
        agent.speed = moveSpeed; 
        
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }
    
    // Coroutine สำหรับการหมุนค้นหา (เปลี่ยนเป็น protected)
    protected IEnumerator SearchRoutine()
    {
        // [Logic SearchRoutine unchanged for brevity]
        isSearching = true;
        originalRotation = transform.rotation;
        agent.isStopped = true; 

        Quaternion leftRot = originalRotation * Quaternion.Euler(0, -searchAngle, 0);
        Quaternion rightRot = originalRotation * Quaternion.Euler(0, searchAngle, 0);

        yield return StartCoroutine(RotateToTarget(leftRot, searchRotationSpeed));
        yield return new WaitForSeconds(waitTime / 2);
        yield return StartCoroutine(RotateToTarget(rightRot, searchRotationSpeed));
        yield return new WaitForSeconds(waitTime / 2);
        yield return StartCoroutine(RotateToTarget(originalRotation, searchRotationSpeed));

        isSearching = false;
    }

    // เมธอดสำหรับหมุนตัว (เปลี่ยนเป็น protected)
    protected IEnumerator RotateToTarget(Quaternion targetRotation, float speed)
    {
        // [Logic RotateToTarget unchanged for brevity]
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            yield return null;
        }
    }

    // เมธอดสำหรับเดิน Patrol (เปลี่ยนเป็น protected virtual เพื่อให้คลาสลูก Override ได้)
    protected virtual void PatrolMovement()
    {
        if (patrolPoints.Count == 0) return;

        // ** สำคัญ: ใช้ Patrol Speed **
        agent.speed = moveSpeed; 
        agent.stoppingDistance = 0.1f; 

        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance && !isSearching)
        {
            if (!agent.isStopped)
                agent.isStopped = true; 

            StartCoroutine(WaitAndGoNextPoint());
        }
        else if (!agent.isStopped)
        {
             // ทำให้ NavMeshAgent เดินไปยังจุดที่กำหนดเสมอ
             agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    // Coroutine สำหรับหยุดรอและเปลี่ยนจุด (เปลี่ยนเป็น protected)
    protected IEnumerator WaitAndGoNextPoint()
    {
        yield return StartCoroutine(SearchRoutine()); 

        GoToNextPoint();
        
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    // เมธอดสำหรับเปลี่ยนจุดถัดไป (เปลี่ยนเป็น protected)
    protected void GoToNextPoint()
    {
        if (patrolPoints.Count == 0) return;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
    }
}
