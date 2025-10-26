using UnityEngine;
using TMPro;
using System.Collections;

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
            textObj.transform.localPosition = new Vector3(0, 0.3f, 0);

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
        if (collected) return;
        collected = true;

        // 🔸 ปิดข้อความ
        if (text3D != null)
            text3D.gameObject.SetActive(false);

        // 🔸 จับภาพ icon ก่อน
        Sprite iconToUse = itemIcon ?? CaptureItemIconAsSprite(gameObject);
        Texture2D iconTexture = SpriteToTexture(iconToUse);

        // 🔸 หาคลัง
        RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
        if (inventory != null)
        {
            // ✅ เพิ่มเข้าคลังทันที (ถือว่าของเข้าตัวแล้ว)
            inventory.AddItem(gameObject, itemID, iconTexture);
        }

        // 🔸 ปิดฟิสิกส์และ collider เพื่อให้ดูเหมือนเก็บไปแล้ว
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // 🔸 ซ่อนวัตถุในฉาก (เหมือนหายเข้า inventory)
        gameObject.SetActive(false);

        // ✅ หน่วงลบจริงออกจากฉาก (เพื่อไม่รบกวน AddItem)
        StartCoroutine(RemoveAfterDelay());
    }

    IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); // หน่วงเล็กน้อยให้ AddItem ทำงานเสร็จ
        Destroy(gameObject);
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

        Vector3 offset = obj.transform.forward * -2f;
        tempCam.transform.position = obj.transform.position + offset;
        tempCam.transform.LookAt(obj.transform.position);

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
