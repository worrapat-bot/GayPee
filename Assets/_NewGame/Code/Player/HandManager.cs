using UnityEngine;

public class HandManager : MonoBehaviour
{
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public float maxCarryWeightPerHand = 5f;

    private InteractableItem leftHandItem;
    private InteractableItem rightHandItem;

    public bool IsLeftHandFree => leftHandItem == null;
    public bool IsRightHandFree => rightHandItem == null;

    public bool CanGrab(InteractableItem item)
    {
        // ตรวจสอบน้ำหนัก
        if (item.holdType == ItemHoldType.OneHand && item.weight > maxCarryWeightPerHand)
            return false;

        if (item.holdType == ItemHoldType.TwoHands && item.weight > maxCarryWeightPerHand * 2)
            return false;

        // ตรวจสอบว่ามือว่างไหม
        if (item.holdType == ItemHoldType.OneHand)
            return IsLeftHandFree || IsRightHandFree;
        else
            return IsLeftHandFree && IsRightHandFree;
    }

    public void GrabItem(InteractableItem item)
    {
        if (!CanGrab(item))
        {
            Debug.Log("ไม่สามารถถือได้: " + item.itemName);
            return;
        }

        if (item.holdType == ItemHoldType.OneHand)
        {
            if (IsRightHandFree)
            {
                rightHandItem = item;
                item.OnGrab(rightHandAnchor);
            }
            else
            {
                leftHandItem = item;
                item.OnGrab(leftHandAnchor);
            }
        }
        else
        {
            leftHandItem = rightHandItem = item;
            item.OnGrab(rightHandAnchor);
        }

        // ถ้ามี QuestItem อยู่ในนี้
        var questItem = item.GetComponent<QuestItem>();
        if (questItem != null)
            questItem.OnPickedUp();
    }

    public void ReleaseAll()
    {
        if (leftHandItem)
        {
            leftHandItem.OnRelease();
            leftHandItem = null;
        }

        if (rightHandItem)
        {
            rightHandItem.OnRelease();
            rightHandItem = null;
        }
    }
}
