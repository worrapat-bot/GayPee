using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

// [อัปเดต] บังคับให้ต้องมี Animator component ด้วย
[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] 
public class EnemyPatrol : MonoBehaviour
{
    // ... (ตัวแปรอื่นๆ เหมือนเดิม: moveSpeed, waitTime, stoppingDistance, patrolPoints, etc.) ...
    // --- (ฉันจะย่อโค้ดส่วนที่ไม่เปลี่ยนแปลง) ---
    [field: Header("Movement Settings")]
    [field: SerializeField] public float moveSpeed { get; set; } = 3.0f;
    [field: SerializeField] public float waitTime { get; set; } = 2.5f;
    [field: SerializeField] public float rotationSpeed { get; set; } = 5.0f;

    [field: Header("Chase & Jumpscare Settings")]
    [field: SerializeField] public float stoppingDistance { get; set; } = 2.0f;
    [field: SerializeField] public float jumpScareDistance { get; set; } = 0.5f;

    [field: Header("Search Settings")]
    [field: SerializeField] public float searchAngle { get; set; } = 45f;
    [field: SerializeField] public float searchRotationSpeed { get; set; } = 100f;

    [field: Header("Patrol Points")]
    [field: SerializeField] public List<Transform> patrolPoints { get; set; } = new List<Transform>();
    
    // --- [เพิ่มส่วนใหม่สำหรับ Monster Animation] ---
    [Header("Jumpscare Animation")]
    [Tooltip("ชื่อของ Trigger Parameter ใน Animator Controller ของมอนสเตอร์")]
    public string jumpscareTriggerName = "DoJumpscare"; 
    // --- [จบส่วนใหม่] ---

    public bool isDetectingPlayer { get; private set; }

    private int currentPointIndex;
    private FieldOfView fieldOfView;
    private NavMeshAgent agent;
    private Animator animator; // [เพิ่ม] ตัวแปรสำหรับ Animator ของมอนสเตอร์
    private bool isSearching = false;
    private bool isCheckingLastKnownPosition = false;
    private bool jumpScareTriggered = false;
    private Vector3? lastKnownPlayerPosition = null;
    private Quaternion originalRotation;

    void Start()
    {
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // [เพิ่ม] ดึง Animator component

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.1f;

        if (patrolPoints.Count == 0)
        {
            enabled = false;
            return;
        }

        currentPointIndex = 0;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    void Update()
    {
        if (jumpScareTriggered) return;

        bool currentlyDetecting = fieldOfView.VisibleTarget != null;

        if (currentlyDetecting)
        {
            isDetectingPlayer = true;
            lastKnownPlayerPosition = fieldOfView.VisibleTarget.position;

            if (Vector3.Distance(transform.position, fieldOfView.VisibleTarget.position) <= jumpScareDistance)
            {
                // ส่ง GameObject ของผู้เล่น (fieldOfView.VisibleTarget.gameObject) ไปด้วย
                HandleJumpScare(fieldOfView.VisibleTarget.gameObject); 
                return;
            }

            ChasePlayer();
        }
        else
        {
            // ... (โค้ดส่วน else เหมือนเดิม) ...
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

    // ... (โค้ดส่วน StopAllAISequences, ChasePlayer, GoToLastKnownPosition, SearchRoutine, RotateToTarget, PatrolMovement, WaitAndGoNextPoint, GoToNextPoint เหมือนเดิม) ...
    // --- (ฉันจะย่อโค้ดส่วนที่ไม่เปลี่ยนแปลง) ---
    
    private void StopAllAISequences()
    {
        StopAllCoroutines();
        isSearching = false;
        isCheckingLastKnownPosition = false;
        if (agent != null && agent.enabled)
            agent.isStopped = true;
    }

    private void ChasePlayer()
    {
        StopAllAISequences();
        isSearching = false;
        isCheckingLastKnownPosition = false;

        Transform target = fieldOfView.VisibleTarget;
        if (agent.isStopped)
            agent.isStopped = false;

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
        yield return StartCoroutine(SearchRoutine());

        lastKnownPlayerPosition = null;
        isCheckingLastKnownPosition = false;

        GoToNextPoint();
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }
    
    private IEnumerator SearchRoutine()
    {
        isSearching = true;
        originalRotation = transform.rotation;

        Quaternion leftRot = originalRotation * Quaternion.Euler(0, -searchAngle, 0);
        Quaternion rightRot = originalRotation * Quaternion.Euler(0, searchAngle, 0);

        yield return StartCoroutine(RotateToTarget(leftRot, searchRotationSpeed));
        yield return new WaitForSeconds(waitTime / 2);
        yield return StartCoroutine(RotateToTarget(rightRot, searchRotationSpeed));
        yield return new WaitForSeconds(waitTime / 2);
        yield return StartCoroutine(RotateToTarget(originalRotation, searchRotationSpeed));

        isSearching = false;
    }

    private IEnumerator RotateToTarget(Quaternion targetRotation, float speed)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            yield return null;
        }
    }

    private void PatrolMovement()
    {
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance && !isSearching)
        {
            if (!agent.isStopped)
                agent.isStopped = true;

            StartCoroutine(WaitAndGoNextPoint());
        }
    }

    private IEnumerator WaitAndGoNextPoint()
    {
        isSearching = true;
        yield return new WaitForSeconds(waitTime);
        GoToNextPoint();
        isSearching = false;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    private void GoToNextPoint()
    {
        if (patrolPoints.Count == 0) return;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
    }

    // --- [ฟังก์ชัน Jumpscare ที่อัปเดตแล้ว] ---
    private void HandleJumpScare(GameObject player)
    {
        if (jumpScareTriggered) return; // [เพิ่ม] ป้องกันการเรียกซ้ำ

        jumpScareTriggered = true;
        StopAllAISequences();

        // [อัปเดต] 1. สั่งให้มอนสเตอร์เล่นแอนิเมชัน Jumpscare
        if (animator != null && !string.IsNullOrEmpty(jumpscareTriggerName))
        {
            animator.SetTrigger(jumpscareTriggerName);
            Debug.Log("JUMPSCARE: Monster Animation Triggered!");
        }
        else
        {
            Debug.LogWarning("JUMPSCARE: Animator หรือ JumpscareTriggerName ไม่ได้ตั้งค่า!");
        }

        // (โค้ดเดิม) 2. สั่ง ragdoll player
        PlayerRagdoll ragdoll = player.GetComponent<PlayerRagdoll>();
        if (ragdoll != null)
            ragdoll.EnableRagdoll();

        // (โค้ดเดิม) 3. กล้องสั่นถ้ามี
        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        if (camShake != null)
            StartCoroutine(camShake.Shake(0.4f, 0.5f));

        // (โค้ดเaดิม) 4. รอให้จบ jumpscare แล้วกลับไป patrol
        StartCoroutine(ResetAfterScare());
    }

    private IEnumerator ResetAfterScare()
    {
        yield return new WaitForSeconds(2f); // รอ 2 วินาที (หรือเท่ากับความยาวแอนิเมชัน)
        jumpScareTriggered = false;
        GoToNextPoint();
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }
}

