using UnityEngine;

public class GhostTrigger : MonoBehaviour
{
    [Tooltip("ใช้ Tag ของตัวละคร (เช่น 'Player') เพื่อให้ trigger ทำงานเฉพาะกับ player")]
    public string activatingTag = "Player";

    [Tooltip("Script หรือ GameObject ที่ต้องการให้ถูกสั่ง (เช่น GhostPush)")]
    public MonoBehaviour[] targets; // ใส่ GhostPush หรือสคริปต์อื่น ๆ ที่เป็น MonoBehaviour

    [Tooltip("ให้ทำงานแค่ครั้งเดียวหรือซ้ำได้")]
    public bool oneShot = true;

    [Tooltip("ถ้าอยากให้มีดีเลย์ก่อนสั่งให้ทำงาน (วินาที)")]
    public float delay = 0f;

    bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (used && oneShot) return;

        if (!string.IsNullOrEmpty(activatingTag))
        {
            if (!other.CompareTag(activatingTag)) return;
        }

        if (delay <= 0f)
        {
            ActivateTargets();
        }
        else
        {
            StartCoroutine(DelayedActivate());
        }

        if (oneShot) used = true;
    }

    System.Collections.IEnumerator DelayedActivate()
    {
        yield return new WaitForSeconds(delay);
        ActivateTargets();
    }

    void ActivateTargets()
    {
        foreach (var t in targets)
        {
            if (t == null) continue;
            t.enabled = true; // ถ้าเป็นสคริปต์ที่ถูกปิดไว้
            // ถ้าสคริปต์มีเมธอดเฉพาะ จะเรียกผ่าน SendMessage (ยืดหยุ่น)
            t.SendMessage("OnTriggered", SendMessageOptions.DontRequireReceiver);
        }
    }

    // ช่วยให้มองเห็นขนาด trigger ใน Scene view
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider bc)
                Gizmos.DrawWireCube(bc.center, bc.size);
            else if (col is SphereCollider sc)
                Gizmos.DrawWireSphere(sc.center, sc.radius);
        }
    }
}
