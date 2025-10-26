using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UniversalDoorSpawner : MonoBehaviour
{
    public enum Requirement { None, Crowbar, Key, MagicStick }

    [System.Serializable]
    public class SpawnableObject
    {
        public string name = "Object"; // ชื่อสำหรับอ้างอิง
        public GameObject prefab; // Prefab ที่จะ Spawn
        public bool shouldSpawn = true; // เลือกว่าจะ Spawn หรือไม่
        public Transform customSpawnPoint; // ตำแหน่ง Spawn เฉพาะ (Optional)
    }

    [Header("Door Settings")]
    public Requirement requirement = Requirement.None;
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public KeyCode interactKey = KeyCode.F;
    public float interactDistance = 3f;

    [Header("🎁 Spawn Settings")]
    public bool enableSpawning = true;
    public List<SpawnableObject> spawnableObjects = new List<SpawnableObject>(); // รายการวัตถุ
    public float spawnDelay = 0.5f; // ดีเลย์หลังเปิดประตูก่อน Spawn
    public bool spawnOnlyOnce = true; // Spawn เพียงครั้งเดียวหรือทุกครั้งที่เปิด
    public Vector3 defaultSpawnOffset = new Vector3(0, 0, 2f); // ระยะห่างจากประตู (default)
    public float spacingBetweenObjects = 1.5f; // ระยะห่างระหว่างวัตถุ

    [Header("🎲 Spawn Options")]
    public bool destroyOnClose = false; // ทำลายวัตถุเมื่อปิดประตู
    public bool alignToSpawnPoint = true; // ใช้ Rotation ของ Spawn Point

    [Header("🔊 Sound Settings")]
    public AudioClip crowbarSound;
    public AudioClip keySound;
    public AudioClip magicStickSound;
    public AudioClip doorOpenSound;
    public AudioClip spawnSound; // เสียงเมื่อ Spawn วัตถุ
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    private bool isOpen = false;
    private bool isMoving = false;
    private bool isUnlocked = false;
    private bool hasSpawned = false;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Camera cam;
    private TextMeshPro text3D;
    private GameObject player;
    private AudioSource audioSource;
    private List<GameObject> spawnedObjects = new List<GameObject>();

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

        Vector3 offset = -transform.forward * 0.8f + Vector3.up * 1.4f;
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
            {
                isMoving = false;

                if (isOpen && enableSpawning)
                {
                    OnDoorFullyOpened();
                }
            }
        }
    }

    void TryOpen()
    {
        if (requirement == Requirement.None)
        {
            ToggleDoor();
            return;
        }

        if (isUnlocked)
        {
            ToggleDoor();
            return;
        }

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

        Debug.Log($"🚪 Door unlocked with {requirement}!");

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

        if (!isOpen && destroyOnClose)
        {
            DestroySpawnedObjects();
        }
    }

    void OnDoorFullyOpened()
    {
        if (spawnOnlyOnce && hasSpawned) return;

        StartCoroutine(SpawnObjectsDelayed());
    }

    IEnumerator SpawnObjectsDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (spawnableObjects == null || spawnableObjects.Count == 0)
        {
            Debug.LogWarning("⚠️ No spawnable objects in list!");
            yield break;
        }

        // เล่นเสียง Spawn
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound, soundVolume);
        }

        SpawnSelectedObjects();

        hasSpawned = true;
        Debug.Log($"🎁 Spawned {spawnedObjects.Count} objects successfully!");
    }

    void SpawnSelectedObjects()
    {
        int spawnedCount = 0;

        for (int i = 0; i < spawnableObjects.Count; i++)
        {
            SpawnableObject spawnObj = spawnableObjects[i];

            // ข้ามถ้าไม่เลือกให้ Spawn หรือไม่มี Prefab
            if (!spawnObj.shouldSpawn || spawnObj.prefab == null)
                continue;

            Vector3 spawnPos = GetSpawnPosition(spawnObj, spawnedCount);
            Quaternion spawnRot = GetSpawnRotation(spawnObj);

            GameObject spawned = Instantiate(spawnObj.prefab, spawnPos, spawnRot);
            spawnedObjects.Add(spawned);

            Debug.Log($"✅ Spawned: {spawnObj.name} at {spawnPos}");
            spawnedCount++;
        }
    }

    Vector3 GetSpawnPosition(SpawnableObject spawnObj, int index)
    {
        // ถ้ามี Custom Spawn Point ให้ใช้
        if (spawnObj.customSpawnPoint != null)
        {
            return spawnObj.customSpawnPoint.position;
        }

        // ถ้าไม่มี ให้ Spawn ที่ตำแหน่งประตู + offset
        Vector3 basePos = transform.position;
        Vector3 offset = transform.TransformDirection(defaultSpawnOffset);

        // เว้นระยะถ้า Spawn หลายอัน
        int totalToSpawn = 0;
        foreach (var obj in spawnableObjects)
        {
            if (obj.shouldSpawn && obj.prefab != null) totalToSpawn++;
        }

        if (totalToSpawn > 1)
        {
            float totalWidth = (totalToSpawn - 1) * spacingBetweenObjects;
            float startOffset = -totalWidth / 2f;
            offset += transform.right * (startOffset + index * spacingBetweenObjects);
        }

        return basePos + offset;
    }

    Quaternion GetSpawnRotation(SpawnableObject spawnObj)
    {
        // ถ้ามี Custom Spawn Point และเปิดใช้ alignment
        if (alignToSpawnPoint && spawnObj.customSpawnPoint != null)
        {
            return spawnObj.customSpawnPoint.rotation;
        }

        return Quaternion.identity;
    }

    void DestroySpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        hasSpawned = false;
        Debug.Log("🗑️ Spawned objects destroyed!");
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

    void OnDrawGizmosSelected()
    {
        if (!enableSpawning || spawnableObjects == null) return;

        int spawnIndex = 0;
        foreach (var spawnObj in spawnableObjects)
        {
            if (!spawnObj.shouldSpawn || spawnObj.prefab == null)
                continue;

            // เลือกสีตาม Checkbox
            Gizmos.color = spawnObj.shouldSpawn ? Color.green : Color.red;

            Vector3 pos;
            if (spawnObj.customSpawnPoint != null)
            {
                pos = spawnObj.customSpawnPoint.position;
                Gizmos.DrawWireSphere(pos, 0.3f);

                // แสดง Forward direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(pos, spawnObj.customSpawnPoint.forward * 0.5f);
            }
            else
            {
                pos = GetSpawnPosition(spawnObj, spawnIndex);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pos, 0.3f);
                Gizmos.DrawLine(transform.position, pos);
            }

            spawnIndex++;
        }
    }
}