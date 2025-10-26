using UnityEngine;
using TMPro;
using System.Collections;

public class UniversalBreakable : MonoBehaviour
{
    public enum Requirement { None, Crowbar, Key, MagicStick }
    public enum DestroyMode { Destroy, ReplaceModel, DisableCollider }

    [Header("?? Breakable Settings")]
    public Requirement requirement = Requirement.None;
    public DestroyMode destroyMode = DestroyMode.Destroy;
    public KeyCode interactKey = KeyCode.F;
    public float interactDistance = 3f;

    [Header("?? Model Replacement (If ReplaceModel)")]
    public GameObject brokenModel; // โมเดลที่จะแทนที่หลังทำลาย
    public float replaceDelay = 0.2f; // ดีเลย์ก่อนเปลี่ยนโมเดล

    [Header("?? Effects")]
    public GameObject breakEffect; // Particle effect เมื่อทำลาย
    public float effectDuration = 2f;

    [Header("?? Sound Settings")]
    public AudioClip crowbarSound; // เสียง Crowbar
    public AudioClip keySound; // เสียง Key
    public AudioClip magicStickSound; // เสียงแท่งมหัศจรรย์
    public AudioClip breakSound; // เสียงทำลาย (ทุกประเภท)
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    [Header("?? Advanced Options")]
    public bool removeItemAfterUse = true; // ลบไอเทมหลังใช้หรือไม่
    public bool sendQuestUpdate = true; // ส่งอัพเดทให้ Quest System

    private bool isBroken = false;
    private Camera cam;
    private TextMeshPro text3D;
    private GameObject player;
    private AudioSource audioSource;

    void Start()
    {
        cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");

        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = soundVolume;

        // Setup 3D Text
        GameObject textObj = new GameObject("BreakableText");
        text3D = textObj.AddComponent<TextMeshPro>();
        text3D.text = GetInteractText();
        text3D.fontSize = 2;
        text3D.color = new Color(1f, 0.3f, 0.3f); // สีแดงเพื่อบ่งบอกว่าทำลายได้
        text3D.alignment = TextAlignmentOptions.Center;
        text3D.enableAutoSizing = false;
        text3D.rectTransform.sizeDelta = new Vector2(3, 1);
        text3D.gameObject.SetActive(false);

        Vector3 offset = transform.forward * 0.8f + Vector3.up * 1.4f;
        text3D.transform.position = transform.position + offset;
        text3D.transform.rotation = transform.rotation;
    }

    void Update()
    {
        if (player == null || isBroken) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        // แสดง text เมื่ออยู่ใกล้
        text3D.gameObject.SetActive(dist < interactDistance);

        // Update text position to face camera
        Vector3 offset = -transform.forward * 0.8f + Vector3.up * 1.4f;
        text3D.transform.position = transform.position + offset;

        // ลองทำลาย
        if (dist < interactDistance && Input.GetKeyDown(interactKey))
        {
            TryBreak();
        }
    }

    string GetInteractText()
    {
        if (requirement == Requirement.None)
        {
            return "Press F to Break";
        }
        else
        {
            return $"Press F (Need {requirement})";
        }
    }

    void TryBreak()
    {
        if (isBroken) return;

        // ถ้าไม่ต้องการไอเทม ทำลายได้เลย
        if (requirement == Requirement.None)
        {
            BreakObject();
            return;
        }

        // ตรวจสอบว่ามีไอเทมหรือไม่
        if (PlayerHasItem(requirement))
        {
            BreakObject();
        }
        else
        {
            string missing = requirement.ToString();
            ShowFloatingText($"You need {missing} to break this!");
        }
    }

    bool PlayerHasItem(Requirement req)
    {
        RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
        if (inventory == null) return false;

        if (!inventory.HasItemInHand()) return false;

        string heldItemName = inventory.GetCurrentItemName();
        string requiredItemName = req.ToString();

        return heldItemName == requiredItemName;
    }

    void BreakObject()
    {
        isBroken = true;

        // เล่นเสียงของไอเทม (Crowbar, Key, etc.)
        AudioClip itemClip = GetItemSound(requirement);
        if (itemClip != null)
        {
            audioSource.PlayOneShot(itemClip, soundVolume);
            Debug.Log($"?? Playing {requirement} sound!");
        }

        // เล่นเสียงทำลาย
        if (breakSound != null)
        {
            audioSource.PlayOneShot(breakSound, soundVolume * 0.7f);
        }

        // แสดง Particle Effect
        if (breakEffect != null)
        {
            GameObject effect = Instantiate(breakEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // ลบไอเทมออกจาก Inventory
        if (removeItemAfterUse && requirement != Requirement.None)
        {
            RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
            if (inventory != null)
            {
                inventory.RemoveCurrentItem();
            }
        }

        // ส่งอัพเดทให้ Quest System
        if (sendQuestUpdate)
        {
            QuestPaperList questList = FindObjectOfType<QuestPaperList>();
            if (questList != null)
            {
                questList.OnDoorUnlocked(requirement.ToString()); // ใช้ชื่อเดิมเพื่อ compatibility
            }
        }

        // ซ่อน Text
        text3D.gameObject.SetActive(false);

        Debug.Log($"?? Object broken with {requirement}!");

        // ทำการทำลายตาม Mode
        StartCoroutine(ExecuteDestroyMode());
    }

    IEnumerator ExecuteDestroyMode()
    {
        yield return new WaitForSeconds(replaceDelay);

        switch (destroyMode)
        {
            case DestroyMode.Destroy:
                Destroy(gameObject);
                break;

            case DestroyMode.ReplaceModel:
                if (brokenModel != null)
                {
                    GameObject broken = Instantiate(brokenModel, transform.position, transform.rotation);
                    broken.transform.localScale = transform.localScale;
                }
                Destroy(gameObject);
                break;

            case DestroyMode.DisableCollider:
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                MeshRenderer rend = GetComponent<MeshRenderer>();
                if (rend != null) rend.enabled = false;
                break;
        }
    }

    AudioClip GetItemSound(Requirement req)
    {
        switch (req)
        {
            case Requirement.Crowbar:
                return crowbarSound;
            case Requirement.Key:
                return keySound;
            case Requirement.MagicStick:
                return magicStickSound;
            default:
                return null;
        }
    }

    void ShowFloatingText(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowTextRoutine(message));
    }

    IEnumerator ShowTextRoutine(string message)
    {
        text3D.text = message;
        yield return new WaitForSeconds(2f);
        text3D.text = GetInteractText();
    }
}