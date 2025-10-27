using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public Camera playerCam;
    public float interactDistance = 3f;
    public KeyCode grabKey = KeyCode.E;

    private HandManager handManager;
    private InteractableItem focusedItem;

    void Start()
    {
        handManager = GetComponent<HandManager>();
    }

    void Update()
    {
        CheckFocus();

        if (Input.GetKeyDown(grabKey))
            TryGrab();

        if (Input.GetKeyDown(KeyCode.R))
            handManager.ReleaseAll();
    }

    void CheckFocus()
    {
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null)
            {
                if (focusedItem != item)
                {
                    ClearFocus();
                    focusedItem = item;
                    focusedItem.OnFocus(true);
                }
                return;
            }
        }

        ClearFocus();
    }

    void ClearFocus()
    {
        if (focusedItem != null)
        {
            focusedItem.OnFocus(false);
            focusedItem = null;
        }
    }

    void TryGrab()
    {
        if (focusedItem != null)
        {
            handManager.GrabItem(focusedItem);
        }
    }
}
