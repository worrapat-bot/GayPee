using System.Collections;
using UnityEngine;

public class ScaryLookDisappearQuick : MonoBehaviour
{
    [Header("การตรวจจับการมอง")]
    public Camera playerCamera;         // กล้องของผู้เล่น
    public float lookDistance = 10f;    // ระยะที่ตรวจจับได้
    public float lookAngle = 10f;       // มุมองศาที่ถือว่ามอง
    public float lookTimeToDisappear = 0.2f; // เวลาที่ต้องมองก่อนจะหาย (วินาที)

    [Header("เสียงผี")]
    public AudioSource scareAudio;
    public AudioClip scareClip;

    private Renderer[] renderers;
    private bool isTriggered = false;
    private float lookTimer = 0f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        renderers = GetComponentsInChildren<Renderer>();

        if (scareAudio == null)
        {
            scareAudio = gameObject.AddComponent<AudioSource>();
            scareAudio.spatialBlend = 1f; // เสียงออกจากจุดวัตถุ
            scareAudio.playOnAwake = false;
        }
    }

    void Update()
    {
        if (isTriggered) return;

        if (IsBeingLookedAt())
        {
            lookTimer += Time.deltaTime;

            // ถ้ามองถึงเวลาที่กำหนด -> หายไป
            if (lookTimer >= lookTimeToDisappear)
            {
                StartCoroutine(DisappearForever());
            }
        }
        else
        {
            // ถ้าไม่ได้มอง ให้รีเซ็ตเวลา
            lookTimer = 0f;
        }
    }

    bool IsBeingLookedAt()
    {
        if (playerCamera == null) return false;

        Vector3 dir = transform.position - playerCamera.transform.position;
        float dist = dir.magnitude;
        if (dist > lookDistance) return false;

        float angle = Vector3.Angle(playerCamera.transform.forward, dir);
        if (angle > lookAngle) return false;

        Ray ray = new Ray(playerCamera.transform.position, dir.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, lookDistance))
        {
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                return true;
        }
        return false;
    }

    IEnumerator DisappearForever()
    {
        isTriggered = true;

        // เล่นเสียงน่ากลัวก่อนหาย
        if (scareClip != null)
            scareAudio.PlayOneShot(scareClip);

        // ปิด Renderer (หาย)
        foreach (Renderer r in renderers)
            r.enabled = false;

        // รอให้เสียงเล่นจบก่อนลบวัตถุ
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerCamera.transform.position, transform.position);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
