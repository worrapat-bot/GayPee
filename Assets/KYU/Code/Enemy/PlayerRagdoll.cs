using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    public Rigidbody[] ragdollBodies;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        foreach (var rb in ragdollBodies)
            rb.isKinematic = true;
    }

    public void EnableRagdoll()
    {
        if (anim != null)
            anim.enabled = false;

        foreach (var rb in ragdollBodies)
            rb.isKinematic = false;

        Debug.Log("Ragdoll Activated!");
    }
}
