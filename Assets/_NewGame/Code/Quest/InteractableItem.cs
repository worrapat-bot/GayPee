using UnityEngine;

public enum ItemHoldType
{
    OneHand,
    TwoHands
}

public class InteractableItem : MonoBehaviour
{
    public string itemName = "Item";
    public ItemHoldType holdType = ItemHoldType.OneHand;
    public float weight = 1f;
    public Color highlightColor = Color.yellow;

    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void OnFocus(bool isFocused)
    {
        if (rend == null) return;
        rend.material.color = isFocused ? highlightColor : originalColor;
    }

    public void OnGrab(Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnRelease()
    {
        transform.SetParent(null);
    }
}
