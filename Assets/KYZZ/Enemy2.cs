using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// สคริปต์นี้ต้องมี FieldOfView, NavMeshAgent และ Animator ติดอยู่ด้วย
[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy2 : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // I. FIELDS & PROPERTIES (Protected for Inheritance)
    // ------------------------------------------------------------------------

    [field: Header("Movement Settings")]
    [Tooltip("ความเร็วในการเดินลาดตระเวน (Base Patrol Speed)")]
    [field: SerializeField] protected float moveSpeed { get; set; } = 3.0f;
    [Tooltip("ความเร็วเมื่อไล่ล่าผู้เล่น (Chase Speed)")]
    [field: SerializeField] protected float chaseSpeed { get; set; } = 7.0f;
    [field: SerializeField] protected float waitTime { get; set; } = 2.5f;
    [field: SerializeField] protected float rotationSpeed { get; set; } = 5.0f;

    [field: Header("Chase Settings")]
    [Tooltip("ระยะห่างขั้นต่ำที่ศัตรูจะหยุดเดินเมื่อไล่ล่าผู้เล่น")]
    [field: SerializeField] protected float stoppingDistance { get; set; } = 0.8f;

    [Header("Animation & Scene Settings")]
    [field: SerializeField] protected float walkSpeedThreshold = 3.0f;
    [field: SerializeField] protected string jumpscareTriggerName = "DoJumpscare";
    [field: SerializeField] protected float fadeToBlackDuration = 2.0f;
    [field: SerializeField] protected string nextSceneName = "JumpScareScene";

    [Header("Search Settings")]
    [field: SerializeField] protected float searchAngle { get; set; } = 45f;
    [field: SerializeField] protected float searchRotationSpeed { get; set; } = 100f;

    [field: Header("Patrol Points")]
    [field: SerializeField] protected List<Transform> patrolPoints { get; set; } = new List<Transform>();

    // 🎵 ========== AUDIO SETTINGS (เพิ่มใหม่) ==========
    [Header("🔊 Audio Settings")]
    [SerializeField] protected AudioClip patrolFootstepSound;
    [SerializeField] protected AudioClip chaseFootstepSound;
    [SerializeField] protected AudioClip detectionSound;
    [SerializeField] protected AudioClip searchSound;
    [SerializeField] protected AudioClip jumpscareSound;
    [SerializeField] protected AudioClip chaseBreathingSound;

    [Header("🔊 Audio Sources")]
    [SerializeField] protected AudioSource footstepAudioSource;
    [SerializeField] protected AudioSource sfxAudioSource;
    [SerializeField] protected AudioSource ambientAudioSource;

    [Header("🔊 Audio Volume")]
    [SerializeField][Range(0f, 1f)] protected float footstepVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] protected float sfxVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] protected float ambientVolume = 0.6f;
    // 🎵 ========== END AUDIO SETTINGS ==========

    public bool isDetectingPlayer { get; protected set; }

    protected int currentPointIndex;
    protected FieldOfView fieldOfView;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected bool isSearching = false;
    protected bool isCheckingLastKnownPosition = false;
    protected Vector3? lastKnownPlayerPosition = null;
    protected Quaternion originalRotation;
    protected bool isGameOverSequenceActive = false;

    // 🎵 Audio State Tracking
    protected bool wasDetectingLastFrame = false;

    // ------------------------------------------------------------------------
    // II. CORE LIFECYCLE & INITIALIZATION
    // ------------------------------------------------------------------------

    protected virtual void Start()
    {
        Debug.Log("✅ EnemyPatrol เริ่มทำงานแล้ว");
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        Time.timeScale = 1.0f;

        if (patrolPoints.Count > 0)
        {
            currentPointIndex = 0;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }

        // 🎵 เริ่มเสียงฝีเท้า
        if (footstepAudioSource != null && patrolFootstepSound != null)
        {
            footstepAudioSource.clip = patrolFootstepSound;
            footstepAudioSource.volume = footstepVolume;
            footstepAudioSource.loop = true;
            footstepAudioSource.Play();
        }
    }

    protected virtual void Update()
    {
        if (isGameOverSequenceActive) return;

        UpdateAnimationSpeed();

        bool currentlyDetecting = fieldOfView.VisibleTarget != null;

        // 🎵 ตรวจจับการเจอผู้เล่นครั้งแรก
        if (currentlyDetecting && !wasDetectingLastFrame)
        {
            if (sfxAudioSource != null && detectionSound != null)
                sfxAudioSource.PlayOneShot(detectionSound, sfxVolume);

            if (footstepAudioSource != null && chaseFootstepSound != null)
            {
                footstepAudioSource.clip = chaseFootstepSound;
                footstepAudioSource.pitch = 1.2f;
                footstepAudioSource.Play();
            }

            if (ambientAudioSource != null && chaseBreathingSound != null)
            {
                ambientAudioSource.clip = chaseBreathingSound;
                ambientAudioSource.volume = ambientVolume;
                ambientAudioSource.loop = true;
                ambientAudioSource.Play();
            }
        }
        else if (!currentlyDetecting && wasDetectingLastFrame)
        {
            if (ambientAudioSource != null) ambientAudioSource.Stop();

            if (footstepAudioSource != null && patrolFootstepSound != null)
            {
                footstepAudioSource.clip = patrolFootstepSound;
                footstepAudioSource.pitch = 1f;
                footstepAudioSource.Play();
            }
        }

        wasDetectingLastFrame = currentlyDetecting;

        if (currentlyDetecting)
        {
            isDetectingPlayer = true;
            lastKnownPlayerPosition = fieldOfView.VisibleTarget.position;

            Transform target = fieldOfView.VisibleTarget;
            if (agent.enabled && target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                ChasePlayer();

                if (distanceToTarget <= stoppingDistance)
                {
                    agent.isStopped = true;
                    RotateTowardsTarget(target.position);
                }
                else if (agent.isStopped && distanceToTarget > stoppingDistance)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }
                else if (agent.isStopped)
                {
                    RotateTowardsTarget(target.position);
                }
            }
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

    // ------------------------------------------------------------------------
    // III. JUMPSCARE LOGIC (Simple Game Over)
    // ------------------------------------------------------------------------

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGameOverSequenceActive)
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            HandleJumpscare();
        }
    }

    protected void HandleJumpscare()
    {
        if (isGameOverSequenceActive) return;

        isGameOverSequenceActive = true;
        StopAllCoroutines();

        // 🎵 หยุดเสียงทั้งหมดและเล่นเสียง Jumpscare
        if (footstepAudioSource != null) footstepAudioSource.Stop();
        if (ambientAudioSource != null) ambientAudioSource.Stop();
        if (sfxAudioSource != null && jumpscareSound != null)
            sfxAudioSource.PlayOneShot(jumpscareSound, 1f);

        // 1. หยุดการเคลื่อนไหวของศัตรู
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 2. ล็อกอนิเมชัน และเริ่ม Sequence
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsHit", true);

        // 3. เริ่ม Coroutine Game Over
        StartCoroutine(GameOverSequence());
    }

    protected IEnumerator GameOverSequence()
    {
        // 1. บังคับเล่นอนิเมชัน Jumpscare (ถ้ามี)
        if (animator != null)
        {
            animator.Play(jumpscareTriggerName);
        }

        // 2. รอการแสดงผล Jumpscare และหน่วงเวลา
        Debug.Log("หน้าจอดำ 2 วินาที และเตรียมเปลี่ยน Scene...");
        yield return new WaitForSeconds(fadeToBlackDuration);

        // 3. เปลี่ยน Scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("▶ โหลดฉากถัดไป: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next Scene Name is not set. Cannot load next scene.");
        }
    }

    // ------------------------------------------------------------------------
    // IV. UTILITY & AI BEHAVIOR
    // ------------------------------------------------------------------------

    protected void UpdateAnimationSpeed()
    {
        if (animator == null || agent == null) return;

        float currentVelocityMagnitude = (agent.enabled && !agent.isStopped) ? agent.velocity.magnitude : 0f;
        float speedValue;

        if (currentVelocityMagnitude < 0.1f)
            speedValue = 0f;
        else if (currentVelocityMagnitude <= walkSpeedThreshold)
            speedValue = Mathf.InverseLerp(0f, walkSpeedThreshold, currentVelocityMagnitude);
        else
            speedValue = 1.0f + ((currentVelocityMagnitude - walkSpeedThreshold) * 0.5f);

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
            animator.SetFloat("Speed", 0f);
    }

    protected virtual void ChasePlayer()
    {
        StopAllCoroutines();
        isSearching = false;
        isCheckingLastKnownPosition = false;

        Transform target = fieldOfView.VisibleTarget;
        agent.speed = chaseSpeed;
        if (agent.isStopped)
            agent.isStopped = false;

        agent.SetDestination(target.position);
    }

    protected IEnumerator GoToLastKnownPosition(Vector3 targetPosition)
    {
        agent.stoppingDistance = 0.05f;
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
        agent.speed = moveSpeed;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    protected IEnumerator SearchRoutine()
    {
        isSearching = true;
        originalRotation = transform.rotation;
        agent.isStopped = true;

        // 🎵 เล่นเสียงค้นหา
        if (sfxAudioSource != null && searchSound != null)
            sfxAudioSource.PlayOneShot(searchSound, sfxVolume);

        Quaternion leftRot = originalRotation * Quaternion.Euler(0, -searchAngle, 0);
        Quaternion rightRot = originalRotation * Quaternion.Euler(0, searchAngle, 0);

        yield return StartCoroutine(RotateToTarget(leftRot, searchRotationSpeed));
        yield return new WaitForSeconds(waitTime / 2);
        yield return StartCoroutine(RotateToTarget(rightRot, searchRotationSpeed));
        yield return new WaitForSeconds(waitTime / 2);
        yield return StartCoroutine(RotateToTarget(originalRotation, searchRotationSpeed));

        isSearching = false;
    }

    protected virtual void PatrolMovement()
    {
        if (patrolPoints.Count == 0) return;

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.05f;

        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance && !isSearching)
        {
            if (!agent.isStopped)
                agent.isStopped = true;

            StartCoroutine(WaitAndGoNextPoint());
        }
        else if (!agent.isStopped)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    protected IEnumerator WaitAndGoNextPoint()
    {
        yield return StartCoroutine(SearchRoutine());
        GoToNextPoint();
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    protected void GoToNextPoint()
    {
        if (patrolPoints.Count == 0) return;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
    }

    protected IEnumerator RotateToTarget(Quaternion targetRotation, float speed)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            yield return null;
        }
    }

    protected void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        }
    }
}