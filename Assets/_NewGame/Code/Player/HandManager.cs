using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Hand Anchors")]
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;

    [Header("Player Settings")]
    public Camera playerCam;
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public KeyCode grabKey = KeyCode.E;

    private InteractableItem leftHandItem;
    private InteractableItem rightHandItem;

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            TryGrabOrRelease();
        }
    }

    private void TryGrabOrRelease()
    {
        if (rightHandItem != null)
        {
            rightHandItem.Release();
            rightHandItem = null;
            return;
        }

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit, interactRange, interactLayer))
        {
            InteractableItem item = hit.collider.GetComponentInParent<InteractableItem>();
            
            // ✅ แก้ไข CS1955: IsGrabbed ถูกใช้เป็น Property (ไม่มีวงเล็บ ())
            if (item != null && !item.IsGrabbed)
            {
                GrabItem(item);
            }
        }
    }

    // 🎯 เรียกจาก PlayerInteractor ได้เลย
    public void GrabItem(InteractableItem item)
    {
        if (item == null) return;

        // ✅ แก้ไข CS1501: ลบ Argument ที่สอง (item.usePhysicsJoint) ออก
        // สมมติว่า TryGrab ถูกออกแบบมารับแค่ Transform เดียว
        bool success = item.TryGrab(rightHandAnchor); 
        
        if (success)
        {
            rightHandItem = item;
            Debug.Log($"Grabbed {item.name}");
        }
    }

    public void ReleaseAll()
    {
        if (leftHandItem != null)
        {
            leftHandItem.Release();
            leftHandItem = null;
        }

        if (rightHandItem != null)
        {
            rightHandItem.Release();
            rightHandItem = null;
        }

        Debug.Log("Released all items.");
    }

    public bool HasFreeHand(bool requiresTwoHands = false)
    {
        if (requiresTwoHands)
            return leftHandItem == null && rightHandItem == null;

        return leftHandItem == null || rightHandItem == null;
    }
}