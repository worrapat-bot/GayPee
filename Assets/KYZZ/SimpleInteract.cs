using UnityEngine;
using TMPro;

public class SimpleItemInteract : MonoBehaviour
{
    [Header("Interact Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Item Settings")]
    public Sprite itemIcon;
    public string itemID = "Flashlight"; // ตั้งชื่อไอเทม

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
        if (collected) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        // ให้ข้อความหันเข้าหากล้อง
        // แก้ให้ Text หันหน้าไปหากล้อง ไม่กลับด้าน
        if (text3D != null)
        {
            text3D.transform.LookAt(cam.transform);
            text3D.transform.Rotate(0, 180f, 0);
        }

        text3D.gameObject.SetActive(dist < interactDistance);

        if (dist < interactDistance && Input.GetKeyDown(interactKey))
        {
            CollectItem();
        }
    }

    void CollectItem()
    {
        collected = true;

        // ✅ ลบ text ทิ้งก่อน
        if (text3D != null)
        {
            Destroy(text3D.gameObject);
            text3D = null;
        }

        RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
        if (inventory != null)
        {
            Sprite iconToUse = itemIcon ?? CaptureItemIconAsSprite(gameObject);
            Texture2D iconTexture = SpriteToTexture(iconToUse);

            // ✅ ส่งข้อมูลไปยังคลัง
            inventory.AddItem(gameObject, itemID, iconTexture);
        }

        // ✅ ปิด Physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // ✅ ปิด script นี้
        this.enabled = false;

        // ✅ ลบ object จริงออกจากโลก (ของจะไม่ตกพื้นอีก)
        Destroy(gameObject, 0.05f);
    }

    Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite == null) return Texture2D.whiteTexture;

        Texture2D source = sprite.texture;
        Rect rect = sprite.rect;
        int x = Mathf.FloorToInt(rect.x);
        int y = Mathf.FloorToInt(rect.y);
        int width = Mathf.FloorToInt(rect.width);
        int height = Mathf.FloorToInt(rect.height);

        if (source.isReadable)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels(x, y, width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
        else
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(source, rt);

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }
    }

    Sprite CaptureItemIconAsSprite(GameObject obj)
    {
        var tempCam = new GameObject("TempIconCam").AddComponent<Camera>();
        tempCam.backgroundColor = Color.clear;
        tempCam.clearFlags = CameraClearFlags.SolidColor;
        tempCam.orthographic = true;
        tempCam.orthographicSize = 0.5f;

        // ✅ ให้กล้องอยู่ด้านหน้าของวัตถุ ในระดับเดียวกัน
        Vector3 offset = obj.transform.forward * -2f; // ถอยหลังจากด้านหน้า
        tempCam.transform.position = obj.transform.position + offset;
        tempCam.transform.LookAt(obj.transform.position); // มองไปที่ตัววัตถุ

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

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return sprite;
    }
}