using UnityEngine;

public class GhostTrigger : MonoBehaviour
{
    public GhostObject[] ghostObjects;  // วัตถุที่จะทำให้ตก
    public string activatingTag = "Player";
    public bool oneShot = true;

    bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag(activatingTag)) return;

        foreach (var obj in ghostObjects)
        {
            if (obj != null)
                obj.Activate();
        }

        used = true;
    }

    // สำหรับดู Trigger ใน Scene
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
