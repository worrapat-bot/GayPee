using UnityEngine;
using TMPro;
using System.Collections;

public class BreakWithCooldown1 : MonoBehaviour
{
    public enum Requirement { None, Crowbar, Key, MagicStick }
    public enum DestroyMode { Destroy, ReplaceModel, DisableCollider }

    [Header("Breakable Settings")]
    public Requirement requirement = Requirement.None;
    public DestroyMode destroyMode = DestroyMode.Destroy;
    public KeyCode interactKey = KeyCode.F;
    public float interactDistance = 3f;

    [Header("Model Replacement (If ReplaceModel)")]
    public GameObject brokenModel;
    public float replaceDelay = 0.2f;

    [Header("Effects")]
    public GameObject breakEffect;
    public float effectDuration = 2f;

    [Header("Sound Settings")]
    public AudioClip crowbarSound;
    public AudioClip keySound;
    public AudioClip magicStickSound;
    public AudioClip breakSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    [Header("Advanced Options")]
    public bool removeItemAfterUse = true;
    public bool sendQuestUpdate = true;

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
        text3D.color = new Color(1f, 0.3f, 0.3f);
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

        // แสดง text ถ้าใกล้
        text3D.gameObject.SetActive(dist < interactDistance);

        // อัพเดตตำแหน่ง text ให้หันกล้อง
        Vector3 offset = -transform.forward * 0.8f + Vector3.up * 1.4f;
        text3D.transform.position = transform.position + offset;

        // ตรวจปุ่มกด
        if (dist < interactDistance && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(TryBreak());
        }
    }

    string GetInteractText()
    {
        if (requirement == Requirement.None)
            return "Press F to Break";
        else
            return $"Press F (Need {requirement})";
    }

    IEnumerator TryBreak()
    {
        if (isBroken) yield break;

        // ตรวจ Requirement
        if (requirement != Requirement.None && !PlayerHasItem(requirement))
        {
            ShowFloatingText($"You need {requirement} to break this!");
            yield break;
        }

        isBroken = true; // ป้องกันการกดซ้ำ

        // เล่นเสียง Item
        AudioClip itemClip = GetItemSound(requirement);
        if (itemClip != null) audioSource.PlayOneShot(itemClip, soundVolume);

        // เล่นเสียง Break
        if (breakSound != null) audioSource.PlayOneShot(breakSound, soundVolume * 0.7f);

        // Particle Effect
        if (breakEffect != null)
        {
            GameObject effect = Instantiate(breakEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // Remove item
        if (removeItemAfterUse && requirement != Requirement.None)
        {
            RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
            if (inventory != null) inventory.RemoveCurrentItem();
        }

        // Update Quest
        if (sendQuestUpdate)
        {
            QuestPaperList questList = FindObjectOfType<QuestPaperList>();
            if (questList != null) questList.OnDoorUnlocked(requirement.ToString());
        }

        // ซ่อน Text
        text3D.gameObject.SetActive(false);

        // รอและทำโหมด Destroy/Replace/Disable
        yield return ExecuteDestroyMode();

        // ถ้าโหมดไม่ Destroy หรือ ReplaceModel ให้สามารถกดใหม่ได้
        if (destroyMode == DestroyMode.DisableCollider)
            isBroken = false;
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

    bool PlayerHasItem(Requirement req)
    {
        RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
        if (inventory == null) return false;
        if (!inventory.HasItemInHand()) return false;

        string heldItemName = inventory.GetCurrentItemName();
        return heldItemName == req.ToString();
    }

    AudioClip GetItemSound(Requirement req)
    {
        switch (req)
        {
            case Requirement.Crowbar: return crowbarSound;
            case Requirement.Key: return keySound;
            case Requirement.MagicStick: return magicStickSound;
            default: return null;
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
        text3D.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        text3D.text = GetInteractText();
    }
}
