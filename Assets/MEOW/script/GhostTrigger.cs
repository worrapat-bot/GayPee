using UnityEngine;

public class GhostTrigger : MonoBehaviour
{
    public GhostObject[] ghostObjects; // วัตถุที่จะทำให้ตก
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
}