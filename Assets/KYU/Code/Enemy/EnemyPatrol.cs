using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // **สำคัญ: ต้อง Import Library นี้**

// สคริปต์นี้ต้องมี FieldOfView และ NavMeshAgent ติดอยู่ด้วย
[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))] 
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการเคลื่อนที่ (NavMeshAgent Speed)")]
    public float moveSpeed = 3.0f; 
    [Tooltip("เวลารวมที่ใช้ในการหมุนและรอเมื่อถึงจุด Patrol")]
    public float waitTime = 2.5f;   
    [Tooltip("ความเร็วในการหมุนเพื่อหันหน้าไปยังจุดถัดไป")]
    public float rotationSpeed = 5.0f; // ใช้สำหรับ Search Rotation เท่านั้น
    
    [Header("Chase & JumpScare Settings")]
    [Tooltip("ระยะห่างขั้นต่ำที่ศัตรูจะหยุดเดินเมื่อไล่ล่าผู้เล่น")]
    public float stoppingDistance = 2.0f; 
    [Tooltip("ระยะห่างที่จะเริ่ม Jump Scare")]
    public float jumpScareDistance = 0.5f; // **ระยะที่ใช้ในการชน/ใกล้ชิด**

    [Header("Scene Management")]
    [Tooltip("ชื่อ Scene ที่จะโหลดเมื่อ Jump Scare ถูกกระตุ้น (Game Over Scene)")]
    public string GameOverSceneName = "GameOverScene"; // **ต้องกำหนดชื่อ Scene ที่ถูกต้อง**
    
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
    private NavMeshAgent agent;

    // สถานะสำหรับ AI ขั้นสูง
    private bool isSearching = false;
    private Quaternion originalPatrolRotation; 
    private Vector3? lastKnownPlayerPosition = null; 
    private bool isCheckingLastKnownPosition = false;
    private bool jumpScareTriggered = false; // **สถานะใหม่: ป้องกันการเล่นซ้ำ**

    void Start()
    {
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>(); 
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.1f; 

        if (patrolPoints.Count == 0)
        {
            enabled = false; 
            return;
        }

        currentPointIndex = 0;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        originalPatrolRotation = transform.rotation; 
    }

    void Update()
    {
        // ถ้า Jump Scare ถูกเล่นแล้ว ให้หยุด Update ทั้งหมด
        if (jumpScareTriggered) return;

        bool currentlyDetecting = fieldOfView.VisibleTarget != null;
        
        if (currentlyDetecting)
        {
            isDetectingPlayer = true;
            lastKnownPlayerPosition = fieldOfView.VisibleTarget.position;

            // ** 1. ตรวจสอบระยะ Jump Scare **
            if (Vector3.Distance(transform.position, fieldOfView.VisibleTarget.position) <= jumpScareDistance)
            {
                HandleJumpScare(); // **เริ่ม Jump Scare ทันที**
                return;
            }
            
            // 2. ถ้ายังไม่ถึงระยะ Jump Scare ให้ไล่ล่าต่อ
            ChasePlayer();
            
        }
        else // ผู้เล่นหลุดจากสายตา
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

    /// <summary>
    /// ** ฟังก์ชันใหม่: จัดการ Jump Scare และเปลี่ยน Scene **
    /// </summary>
    private void HandleJumpScare()
    {
        if (jumpScareTriggered) return;

        // 1. ตั้งค่าสถานะป้องกันการเล่นซ้ำ
        jumpScareTriggered = true;
        
        // 2. หยุด AI ทั้งหมด
        StopAllAISequences();
        agent.enabled = false; // ปิด NavMeshAgent เพื่อให้ศัตรูหยุดนิ่ง
        
        // 3. เริ่มเปลี่ยน Scene ทันที
        if (!string.IsNullOrEmpty(GameOverSceneName))
        {
            // ** เปลี่ยน Scene ทันที **
            SceneManager.LoadScene(GameOverSceneName);
            Debug.Log($"JUMPSCARE TRIGGERED! Loading Scene: {GameOverSceneName}");
        }
        else
        {
            Debug.LogError("GameOverSceneName ไม่ถูกกำหนด! ไม่สามารถเปลี่ยน Scene ได้");
        }
    }


    // ------------------------------------------------------------------------
    // (ส่วน Logic AI อื่นๆ ที่ใช้ NavMeshAgent เหมือนเดิม)
    // ------------------------------------------------------------------------

    private void StopAllAISequences()
    {
        StopAllCoroutines();
        isSearching = false;
        isCheckingLastKnownPosition = false;
        if (agent != null && agent.enabled && agent.hasPath) 
        {
            agent.isStopped = true;
        }
    }

    private void ChasePlayer()
    {
        StopAllCoroutines(); 
        isSearching = false;
        isCheckingLastKnownPosition = false;
        
        Transform target = fieldOfView.VisibleTarget;
        
        if (agent.isStopped) 
        {
            agent.isStopped = false;
        }

        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);
    }

    private IEnumerator GoToLastKnownPosition(Vector3 targetPosition)
    {
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
        lastKnownPlayerPosition = null; 

        // 360 Degree Search Logic
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, 180, 0);
        yield return StartCoroutine(RotateToTarget(targetRotation, finalSearchSpeed));
        yield return StartCoroutine(RotateToTarget(currentRotation, finalSearchSpeed)); 
        
        isCheckingLastKnownPosition = false;
        
        GoToNextPoint();
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }


    private void PatrolMovement()
    {
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance && !isSearching) 
        {
            if (agent.isStopped == false) 
            {
                agent.isStopped = true;
            }
            
            originalPatrolRotation = transform.rotation;
            StartCoroutine(SearchRoutine());
        } 
        else if (isSearching)
        {
            return;
        }
        else 
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
        }
    }
    
    private IEnumerator SearchRoutine()
    {
        isSearching = true;
        
        Quaternion leftTarget = originalPatrolRotation * Quaternion.Euler(0, -searchAngle, 0);
        yield return StartCoroutine(RotateToTarget(leftTarget, searchRotationSpeed));
        
        yield return new WaitForSeconds(waitTime / 2); 
        
        Quaternion rightTarget = originalPatrolRotation * Quaternion.Euler(0, searchAngle, 0);
        yield return StartCoroutine(RotateToTarget(rightTarget, searchRotationSpeed));
        
        yield return new WaitForSeconds(waitTime / 2); 
        
        yield return StartCoroutine(RotateToTarget(originalPatrolRotation, searchRotationSpeed));
        
        yield return new WaitForSeconds(3f);
        
        isSearching = false;
        GoToNextPoint();
        
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    private IEnumerator RotateToTarget(Quaternion targetRotation, float rotationRate)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationRate * Time.deltaTime 
            );
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    
    private void GoToNextPoint()
    {
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
    }
}
