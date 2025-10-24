using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GhostObject : MonoBehaviour
{
    [Header("Timing")]
    public float delayBeforeShake = 2f; // เวลารอก่อนเริ่มสั่น
    public float shakeDuration = 1f;    // เวลาสั่นก่อนตก

    [Header("Shake")]
    public float shakeAmount = 0.05f;   // ระยะสั่น

    [Header("Push / Fall")]
    public float pushForce = 5f;        // แรงผลักตอนตก
    public float torqueForce = 3f;      // แรงหมุน
    public Vector3 pushDirection = new Vector3(1, 0, 1); // ทิศทางผลัก

    Rigidbody rb;
    Vector3 originalPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // เริ่มนิ่ง
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // ป้องกันทะลุ
        originalPos = transform.localPosition;
    }

    void OnEnable()
    {
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
        rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
    }
}
