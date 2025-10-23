using UnityEngine;
using TMPro;

public class SimpleItemInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Transform handPoint;
    public Texture2D itemIcon;

    private Camera cam;
    private bool collected = false;
    private TextMeshPro text3D;

    void Start()
    {
        cam = Camera.main;

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

        text3D.transform.rotation = Quaternion.LookRotation(text3D.transform.position - cam.transform.position);
        text3D.gameObject.SetActive(dist < interactDistance);

        // ✅ เมื่อกด E เก็บของ
        if (dist < interactDistance && Input.GetKeyDown(interactKey))
        {
            collected = true;
            text3D.gameObject.SetActive(false);

            // ✅ หา inventory
            RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();

            if (inventory != null)
            {
                // ✅ ถ่ายภาพไอคอนถ้ายังไม่มี
                Texture2D iconToUse = itemIcon;
                if (iconToUse == null)
                    iconToUse = CaptureItemIcon(gameObject);

                // ✅ เรียก AddItem เพียงครั้งเดียว
                inventory.AddItem(gameObject, gameObject.name, iconToUse);
            }
        }
    }

    private Texture2D CaptureItemIcon(GameObject obj)
    {
        var tempCam = new GameObject("TempIconCam").AddComponent<Camera>();
        tempCam.backgroundColor = Color.clear;
        tempCam.orthographic = true;
        tempCam.orthographicSize = 0.5f;
        tempCam.transform.position = obj.transform.position + Vector3.back * 2f;
        tempCam.transform.LookAt(obj.transform);

        RenderTexture rt = new RenderTexture(128, 128, 16);
        tempCam.targetTexture = rt;
        Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);

        tempCam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        Destroy(rt);
        Destroy(tempCam.gameObject);

        return tex;
    }
}