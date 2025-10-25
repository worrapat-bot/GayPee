using UnityEngine;
using TMPro;
using System.Collections;

public class UniversalDoorByMeow : MonoBehaviour
{
    public enum Requirement { None, Crowbar, Key, MagicStick }

    [Header("Door Settings")]
    public Requirement requirement = Requirement.None;
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public KeyCode interactKey = KeyCode.F;
    public float interactDistance = 3f;

    [Header("🔊 Sound Settings")]
    public AudioClip crowbarSound;
    public AudioClip keySound;
    public AudioClip magicStickSound;
    public AudioClip doorOpenSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    private bool isOpen = false;
    private bool isMoving = false;
    private bool isUnlocked = false;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Camera cam;
    private TextMeshPro text3D;
    private GameObject player;
    private AudioSource audioSource;

    void Start()
    {
        cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");
        if (doorPivot == null) doorPivot = transform;
        closedRot = doorPivot.localRotation;
        openRot = Quaternion.Euler(0, openAngle, 0) * closedRot;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = soundVolume;

        GameObject textObj = new GameObject("DoorText");
        text3D = textObj.AddComponent<TextMeshPro>();
        text3D.text = "Press F to Open";
        text3D.fontSize = 2;
        text3D.color = new Color(0.2f, 1f, 1f);
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
        if (player == null) return;
        float dist = Vector3.Distance(player.transform.position, transform.position);

        text3D.gameObject.SetActive(dist < interactDistance && !isUnlocked);

        Vector3 offset = -transform.forward * 0.8f;
        text3D.transform.position = transform.position + offset;

        if (dist < interactDistance && Input.GetKeyDown(interactKey))
        {
            TryOpen();
        }

        if (isMoving)
        {
            Quaternion targetRot = isOpen ? openRot : closedRot;
            doorPivot.localRotation = Quaternion.Lerp(doorPivot.localRotation, targetRot, Time.deltaTime * openSpeed);
            if (Quaternion.Angle(doorPivot.localRotation, targetRot) < 0.5f)
                isMoving = false;
        }
    }

    void TryOpen()
    {
        if (requirement == Requirement.None)
        {
            ToggleDoor();
            return;
        }

        if (isUnlocked) return;

        if (!isOpen)
        {
            if (PlayerHasItem(requirement))
            {
                UnlockAndOpen();
            }
            else
            {
                string missing = requirement.ToString();
                ShowFloatingText($"Go find your {missing} first, genius!");
            }
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

    void UnlockAndOpen()
    {
        isUnlocked = true;
        isOpen = true;
        isMoving = true;

        AudioClip unlockClip = GetUnlockSound(requirement);
        if (unlockClip != null)
        {
            audioSource.PlayOneShot(unlockClip, soundVolume);
            Debug.Log($"🔊 Playing {requirement} sound!");
        }

        if (doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound, soundVolume * 0.7f);
        }

        RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
        if (inventory != null)
        {
            inventory.RemoveCurrentItem();
        }

        text3D.gameObject.SetActive(false);

        Debug.Log($"🔓 Door unlocked with {requirement}!");

        // ✅ แจ้ง Quest List ว่าเปิดประตูแล้ว
        QuestPaperList questList = FindObjectOfType<QuestPaperList>();
        if (questList != null)
        {
            questList.OnDoorUnlocked(requirement.ToString());
        }
    }

    AudioClip GetUnlockSound(Requirement req)
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

    void ToggleDoor()
    {
        isOpen = !isOpen;
        isMoving = true;
        text3D.text = isOpen ? "Press F to Close" : "Press F to Open";

        if (doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound, soundVolume * 0.5f);
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
        text3D.text = isOpen ? "Press F to Close" : "Press F to Open";
    }
}