using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class GhostObject : MonoBehaviour
{
    [Header("Timing")]
    public float delayBeforeShake = 0.5f; // เวลารอก่อนเริ่มสั่น
    public float shakeDuration = 0.5f;    // เวลาสั่นก่อนตก

    [Header("Shake")]
    public float shakeAmount = 0.05f;     // ความแรงสั่น

    [Header("Push / Fall")]
    public float pushForce = 5f;
    public float torqueForce = 3f;
    public Vector3 pushDirection = new Vector3(1, 0, 1);

    [Header("Sound")]
    public AudioClip fallClip;
    [Range(0f, 1f)] public float volume = 1f;

    Rigidbody rb;
    AudioSource audioSource;
    Vector3 originalPos;
    bool hasFallen = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        originalPos = transform.localPosition;
    }

    // เรียกจาก Trigger
    public void Activate()
    {
        if (!hasFallen)
        {
            hasFallen = true;
            Invoke(nameof(StartShake), delayBeforeShake);
        }
    }

    void StartShake()
    {
        InvokeRepeating(nameof(DoShake), 0f, 0.01f);
        Invoke(nameof(Fall), shakeDuration);
    }

    void DoShake()
    {
        transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
    }

    void Fall()
    {
        CancelInvoke(nameof(DoShake));
        transform.localPosition = originalPos;

        rb.isKinematic = false;
        rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fallClip != null)
            audioSource.PlayOneShot(fallClip, volume);
    }
}
