using UnityEngine;
using TMPro;

public class SimpleItemInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Transform handPoint;

    private Camera cam;
    private bool collected = false;
    private static GameObject heldItem;
    private TextMeshPro text3D;

    void Start()
    {
        cam = Camera.main;

        // ✅ ถ้ายังไม่มี TextMeshPro ให้สร้างใหม่อัตโนมัติ
        if (GetComponentInChildren<TextMeshPro>() == null)
        {
            GameObject textObj = new GameObject("CollectText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0, 1.0f, 0);

            text3D = textObj.AddComponent<TextMeshPro>();
            text3D.text = "Press E to Collect";
            text3D.fontSize = 2;
            text3D.color = Color.yellow;
            text3D.alignment = TextAlignmentOptions.Center;
            text3D.enableAutoSizing = false;
        }
        else
        {
            text3D = GetComponentInChildren<TextMeshPro>();
        }

        text3D.gameObject.SetActive(false);
    }

    void Update()
    {
        if (collected || handPoint == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        // ✅ ให้ข้อความหันเข้าหากล้องเสมอ (Billboard)
        text3D.transform.rotation = Quaternion.LookRotation(text3D.transform.position - cam.transform.position);

        // ✅ แสดงข้อความเฉพาะเมื่ออยู่ในระยะ และไม่ได้ถือของอื่น
        text3D.gameObject.SetActive(dist < interactDistance && heldItem == null);

        // ✅ เมื่อกด E และอยู่ในระยะ
        if (dist < interactDistance && Input.GetKeyDown(interactKey) && heldItem == null)
        {
            collected = true;
            heldItem = gameObject;
            text3D.gameObject.SetActive(false);
            transform.SetParent(handPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
