using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class GhostObject : MonoBehaviour
{
    [Header("Shake Settings")]
    public float delayBeforeShake = 0.3f;  // รอก่อนสั่น
    public float shakeDuration = 0.4f;     // เวลาสั่น
    public float shakeAmount = 0.05f;      // ความแรงสั่น

    [Header("Fall Settings")]
    public float pushForce = 3f;           // แรงผลัก
    public Vector3 pushDirection = new Vector3(1, 0, 1);

    [Header("Sound Settings")]
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
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        originalPos = transform.localPosition;
    }

    // เรียกจาก Trigger
    public void Activate()
    {
        if (hasFallen) return;
        hasFallen = true;
        Invoke(nameof(StartShake), delayBeforeShake);
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
        rb.AddTorque(Random.insideUnitSphere * pushForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fallClip != null)
            audioSource.PlayOneShot(fallClip, volume);
    }
}
