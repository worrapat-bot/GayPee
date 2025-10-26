using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip punchClip;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Settings")]
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private AudioSource audioSource;
    private CharacterController controller;
    private float stepCycle;
    private float nextStep;
    private bool isGroundedPrev;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        stepCycle = 0f;
        nextStep = stepCycle / 2f;
    }

    void Update()
    {
        HandleMovementSound();
        HandleLandingSound();
    }

    private void HandleMovementSound()
    {
        if (!controller.isGrounded) return;

        Vector3 velocity = controller.velocity;
        float speed = velocity.magnitude;

        if (speed > 0.1f)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float stepInterval = isRunning ? runStepInterval : walkStepInterval;
            stepCycle += (speed + (isRunning ? 1f : 0.5f)) * Time.deltaTime;

            if (stepCycle > nextStep)
            {
                nextStep = stepCycle + stepInterval;
                PlayFootStepAudio(isRunning);
            }
        }
        else
        {
            stepCycle = 0f;
            nextStep = 0f;
        }
    }

    private void HandleLandingSound()
    {
        bool groundedNow = controller.isGrounded;
        if (groundedNow && !isGroundedPrev)
        {
            PlaySound(landClip);
        }
        isGroundedPrev = groundedNow;
    }

    private void PlayFootStepAudio(bool isRunning)
    {
        AudioClip clip = isRunning ? runClip : walkClip;
        if (clip == null) return;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip);
    }

    public void PlayPunchSound()
    {
        PlaySound(punchClip);
    }

    public void PlayJumpSound()
    {
        PlaySound(jumpClip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip);
    }
}
