using UnityEngine;
using System.Collections; // ต้องมีสำหรับ Coroutine

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class InteractableItem : MonoBehaviour
{
    [Header("🔸 Interaction Settings")]
    public string itemName = "Item";
    public float interactDistance = 3f;
    public float weight = 1f;
    public bool isQuestItem = false;
    public string questID = "";

    [Header("🔸 Visual Feedback")]
    public Renderer highlightRenderer;
    public Color highlightColor = Color.yellow;
    private Color defaultColor;

    [Header("🔸 Grab Settings")]
    public bool canBeHeld = true;
    public bool usePhysicsJoint = false;
    public Transform holdPointOverride;

    private bool isFocused = false;
    private bool isGrabbed = false;
    private Rigidbody rb;
    private QuestManager questManager;
    
    // ✅ NEW PROPERTY: สำหรับ HandManager.cs เรียกใช้ (แก้ไขปัญหา CS1955)
    public bool IsGrabbed => isGrabbed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (highlightRenderer != null)
            defaultColor = highlightRenderer.material.color;

        // ต้องตรวจสอบ QuestManager.Instance ว่ามีอยู่จริงหรือไม่
        // if (QuestManager.Instance != null)
        //     questManager = QuestManager.Instance;
        // หรือใช้โค้ดเดิมของคุณ:
        questManager = QuestManager.Instance;
    }

    // ---------------------------
    // 🔹 Focus / Unfocus (แก้ไข CS1501: ไม่รับ Argument)
    // ---------------------------
    public void OnFocus() // ✅ ไม่รับ Argument
    {
        if (isFocused) return;
        isFocused = true;

        if (highlightRenderer != null)
            highlightRenderer.material.color = highlightColor;

        Debug.Log($"Focus on {itemName}");
        if (isQuestItem && questManager != null)
            questManager.NotifyFocusQuestItem(questID, itemName);
    }

    public void OnUnfocus() // ✅ ไม่รับ Argument
    {
        if (!isFocused) return;
        isFocused = false;

        if (highlightRenderer != null)
            highlightRenderer.material.color = defaultColor;
    }

    // ---------------------------
    // 🔹 ถือของ
    // ---------------------------
    public bool TryGrab(Transform hand)
    {
        if (!canBeHeld || isGrabbed)
            return false;

        isGrabbed = true;

        rb.isKinematic = false;
        rb.useGravity = false;

        // ล็อกตำแหน่งให้อยู่กับมือด้วยแรงทางฟิสิกส์ (ไม่หลุด)
        StartCoroutine(SmoothFollowHand(hand));

        Debug.Log($"Grabbed {itemName}");
        return true;
    }

    private System.Collections.IEnumerator SmoothFollowHand(Transform hand)
    {
        float followSpeed = 20f;
        float rotateSpeed = 10f;

        while (isGrabbed && hand != null)
        {
            rb.velocity = (hand.position - transform.position) * followSpeed;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, hand.rotation, Time.deltaTime * rotateSpeed));
            yield return null;
        }
        
        // หาก Coroutine หยุดทำงานโดยไม่ได้เรียก Release ให้มั่นใจว่า isGrabbed = false
        if (isGrabbed)
        {
            isGrabbed = false;
            rb.useGravity = true;
        }
    }

    public void Release()
    {
        if (!isGrabbed) return;

        isGrabbed = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        // หยุด Coroutine SmoothFollowHand ทันที
        StopCoroutine("SmoothFollowHand");

        Debug.Log($"Released {itemName}");
    }


    // ---------------------------
    // 🔹 สำหรับ Quest
    // ---------------------------
    public void Interact()
    {
        if (isQuestItem && questManager != null)
        {
            questManager.NotifyItemCollected(questID, itemName);
            questManager.UpdateQuestProgress(questID);
        }
    }
}