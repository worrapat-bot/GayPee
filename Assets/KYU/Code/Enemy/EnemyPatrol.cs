using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 3.0f;
    [SerializeField] protected float chaseSpeed = 7.0f;
    [SerializeField] protected float waitTime = 2.5f;
    [SerializeField] protected float rotationSpeed = 5.0f;

    [Header("Chase Settings")]
    [SerializeField] protected float stoppingDistance = 0.8f;

    [Header("Animation & Scene Settings")]
    [SerializeField] protected float walkSpeedThreshold = 3.0f;
    [SerializeField] protected string jumpscareTriggerName = "DoJumpscare";
    [SerializeField] protected float fadeToBlackDuration = 2.0f;
    [SerializeField] protected string nextSceneName = "JumpScareScene";

    [Header("Search Settings")]
    [SerializeField] protected float searchAngle = 45f;
    [SerializeField] protected float searchRotationSpeed = 100f;

    [Header("Patrol Points")]
    [SerializeField] protected List<Transform> patrolPoints = new List<Transform>();

    [Header("🔊 Sound Settings")]
    [SerializeField] private AudioSource sfxSource;         // เอฟเฟกต์ทั่วไป
    [SerializeField] private AudioSource musicSource;       // เพลงไล่ล่า
    [SerializeField] private List<AudioClip> footstepClips; // เสียงเดิน
    [SerializeField] private AudioClip alertClip;           // เจอผู้เล่น
    [SerializeField] private AudioClip searchClip;          // ค้นหา
    [SerializeField] private AudioClip jumpscareClip;       // จับผู้เล่น
    [SerializeField] private AudioClip chaseMusicClip;      // เพลงลุ้นละทึก
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private float chaseMusicVolume = 0.6f;
    [SerializeField] private float chaseMusicMaxDistance = 20f;
    [SerializeField] private float chaseMusicMinDistance = 5f;

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
    protected bool isChaseMusicPlaying = false;
    protected bool wasDetectingLastFrame = false;
    private float nextFootstepTime = 0f;
    private Transform player;

    protected virtual void Start()
    {
        fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
        }

        // ✅ บังคับให้เสียงเป็น 3D (กันลืมตั้งใน Inspector)
        sfxSource.spatialBlend = 1f;
        musicSource.spatialBlend = 1f;

        sfxSource.minDistance = 3f;
        sfxSource.maxDistance = 20f;
        musicSource.minDistance = chaseMusicMinDistance;
        musicSource.maxDistance = chaseMusicMaxDistance;

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        if (patrolPoints.Count > 0)
        {
            currentPointIndex = 0;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    protected virtual void Update()
    {
        if (isGameOverSequenceActive) return;

        UpdateAnimationSpeed();

        bool currentlyDetecting = fieldOfView.VisibleTarget != null;

        if (currentlyDetecting && !wasDetectingLastFrame)
        {
            PlaySFX(alertClip);
            PlayChaseMusic();
        }

        if (!currentlyDetecting && wasDetectingLastFrame)
        {
            StopChaseMusic();
        }

        wasDetectingLastFrame = currentlyDetecting;
        isDetectingPlayer = currentlyDetecting;

        if (currentlyDetecting)
        {
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
            }
        }
        else
        {
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

        HandleFootsteps();
        UpdateChaseMusicVolume();
    }

    // 🎧 ปรับเสียงไล่ล่าตามระยะผู้เล่น
    protected void UpdateChaseMusicVolume()
    {
        if (!isChaseMusicPlaying || musicSource == null || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float t = Mathf.InverseLerp(chaseMusicMaxDistance, chaseMusicMinDistance, distance);
        musicSource.volume = Mathf.Lerp(0f, chaseMusicVolume, t);
    }

    protected void HandleFootsteps()
    {
        if (Time.time < nextFootstepTime) return;
        if (footstepClips.Count == 0) return;
        if (!agent.enabled || agent.isStopped) return;

        float velocity = agent.velocity.magnitude;
        if (velocity > 0.1f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Count)];
            sfxSource.PlayOneShot(clip);
            nextFootstepTime = Time.time + footstepInterval * Mathf.Lerp(1.0f, 0.5f, velocity / chaseSpeed);
        }
    }

    protected void PlayChaseMusic()
    {
        if (musicSource == null || chaseMusicClip == null || isChaseMusicPlaying) return;
        musicSource.clip = chaseMusicClip;
        musicSource.volume = 0f; // เริ่มเบา แล้วให้ UpdateChaseMusicVolume ค่อย ๆ ปรับ
        musicSource.loop = true;
        musicSource.Play();
        isChaseMusicPlaying = true;
    }

    protected void StopChaseMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        isChaseMusicPlaying = false;
    }

    protected void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

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
        StopChaseMusic();
        PlaySFX(jumpscareClip);

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsHit", true);
        StartCoroutine(GameOverSequence());
    }

    protected IEnumerator GameOverSequence()
    {
        if (animator != null)
        {
            animator.Play(jumpscareTriggerName);
        }

        yield return new WaitForSeconds(fadeToBlackDuration);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next Scene Name is not set.");
        }
    }

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
        PlaySFX(searchClip);
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
        PlaySFX(searchClip);
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
