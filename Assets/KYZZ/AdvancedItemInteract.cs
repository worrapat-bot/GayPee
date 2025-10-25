using UnityEngine;
using TMPro;
using System.Collections;

public class AdvancedItemInteract : MonoBehaviour
{
    [Header("🎯 Interaction Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask playerLayer;
    public bool requireLineOfSight = true;

    [Header("📦 Item Settings")]
    public Sprite itemIcon;
    public string itemID = "Item";
    [TextArea(2, 4)]
    public string itemDescription = "An item";
    public bool destroyOnCollect = true;
    public AudioClip collectSound;

    [Header("💫 Visual Effects")]
    public bool enableFloating = true;
    public float floatHeight = 0.2f;
    public float floatSpeed = 1f;
    public bool enableRotation = true;
    public float rotationSpeed = 45f;
    public bool enableGlow = true;
    public Color glowColor = Color.yellow;
    [Range(0f, 2f)]
    public float glowIntensity = 1f;

    [Header("📝 Text Display")]
    public bool useCustomText = false;
    [TextArea(1, 2)]
    public string customInteractText = "Press E to Collect";
    public float textHeight = 1.0f;
    public float textSize = 2f;
    public Color textColor = Color.yellow;
    public Color textOutlineColor = Color.black;
    [Range(0f, 1f)]
    public float textOutlineWidth = 0.2f;

    [Header("🔊 Audio Settings")]
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    public bool playSoundOnProximity = false;
    public AudioClip proximitySound;

    [Header("⚡ Advanced Options")]
    public bool useRaycastCheck = true;
    public bool showDebugInfo = false;
    public GameObject[] objectsToDisableOnCollect;
    public UnityEngine.Events.UnityEvent onCollectEvent;

    // Private variables
    private Camera cam;
    private bool collected = false;
    private TextMeshPro textDisplay;
    private GameObject player;
    private AudioSource audioSource;
    private Vector3 startPosition;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] glowMaterials;
    private bool playerInRange = false;
    private bool hasPlayedProximitySound = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        cam = Camera.main;
        startPosition = transform.position;

        if (playerLayer.value == 0)
        {
            playerLayer = LayerMask.GetMask("Default");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = soundVolume;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = interactDistance * 2f;

        CreateTextDisplay();

        if (enableGlow)
        {
            SetupGlowEffect();
        }

        if (showDebugInfo)
        {
            Debug.Log($"📦 Item '{itemID}' initialized at {transform.position}");
        }
    }

    void CreateTextDisplay()
    {
        TextMeshPro existingText = GetComponentInChildren<TextMeshPro>();

        if (existingText == null)
        {
            GameObject textObj = new GameObject("InteractText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0, textHeight, 0);
            textObj.transform.localRotation = Quaternion.identity;

            textDisplay = textObj.AddComponent<TextMeshPro>();
        }
        else
        {
            textDisplay = existingText;
        }

        string displayText = useCustomText ? customInteractText : $"Press {interactKey} to Collect";
        textDisplay.text = displayText;
        textDisplay.fontSize = textSize;
        textDisplay.color = textColor;
        textDisplay.alignment = TextAlignmentOptions.Center;
        textDisplay.enableAutoSizing = false;
        textDisplay.fontStyle = FontStyles.Bold;

        if (textOutlineWidth > 0f)
        {
            textDisplay.outlineColor = textOutlineColor;
            textDisplay.outlineWidth = textOutlineWidth;
        }

        textDisplay.gameObject.SetActive(false);
    }

    void SetupGlowEffect()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        originalMaterials = new Material[renderers.Length];
        glowMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            glowMaterials[i] = new Material(renderers[i].material);

            glowMaterials[i].EnableKeyword("_EMISSION");
            glowMaterials[i].SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    void Update()
    {
        if (collected) return;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactDistance;

        bool canSee = true;
        if (requireLineOfSight && playerInRange)
        {
            canSee = CheckLineOfSight();
        }

        bool canInteract = playerInRange && canSee;

        if (playSoundOnProximity && proximitySound != null)
        {
            if (playerInRange && !hasPlayedProximitySound)
            {
                audioSource.PlayOneShot(proximitySound, soundVolume * 0.5f);
                hasPlayedProximitySound = true;
            }
            else if (!playerInRange)
            {
                hasPlayedProximitySound = false;
            }
        }

        if (textDisplay != null)
        {
            textDisplay.gameObject.SetActive(canInteract);
            if (canInteract)
            {
                Vector3 directionToCamera = cam.transform.position - textDisplay.transform.position;
                textDisplay.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }

        if (enableGlow && renderers != null && renderers.Length > 0)
        {
            UpdateGlowEffect(canInteract);
        }

        UpdateVisualEffects();

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            CollectItem();
        }

        if (showDebugInfo)
        {
            Debug.DrawLine(transform.position, player.transform.position, canInteract ? Color.green : Color.red);
        }
    }

    bool CheckLineOfSight()
    {
        if (!useRaycastCheck) return true;

        Vector3 directionToPlayer = player.transform.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, interactDistance))
        {
            if (hit.collider.gameObject == player || hit.collider.transform.IsChildOf(player.transform))
            {
                return true;
            }
        }

        return false;
    }

    void UpdateVisualEffects()
    {
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        if (enableRotation)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void UpdateGlowEffect(bool glowing)
    {
        if (renderers == null || glowMaterials == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material = glowing ? glowMaterials[i] : originalMaterials[i];
            }
        }
    }

    void CollectItem()
    {
        if (collected) return;

        collected = true;

        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound, soundVolume);
        }

        RadialInventoryVertical inventory = FindObjectOfType<RadialInventoryVertical>();
        if (inventory != null)
        {
            Sprite iconToUse = itemIcon != null ? itemIcon : CaptureItemIcon();
            Texture2D iconTexture = SpriteToTexture(iconToUse);

            inventory.AddItem(gameObject, itemID, iconTexture);

            if (showDebugInfo)
            {
                Debug.Log($"✅ Collected '{itemID}' and added to inventory");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ RadialInventoryVertical not found!");
        }

        if (objectsToDisableOnCollect != null)
        {
            foreach (GameObject obj in objectsToDisableOnCollect)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        onCollectEvent?.Invoke();

        if (textDisplay != null)
        {
            textDisplay.gameObject.SetActive(false);
        }

        if (destroyOnCollect)
        {
            StartCoroutine(DestroyAfterSound());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator DestroyAfterSound()
    {
        if (collectSound != null)
        {
            yield return new WaitForSeconds(collectSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(gameObject);
    }

    Sprite CaptureItemIcon()
    {
        GameObject camObj = new GameObject("TempIconCamera");
        Camera tempCam = camObj.AddComponent<Camera>();

        tempCam.backgroundColor = Color.clear;
        tempCam.clearFlags = CameraClearFlags.SolidColor;
        tempCam.orthographic = true;
        tempCam.orthographicSize = 0.5f;
        tempCam.nearClipPlane = 0.1f;
        tempCam.farClipPlane = 5f;

        Vector3 iconCamPos = transform.position + Vector3.back * 2f + Vector3.up * 0.5f;
        tempCam.transform.position = iconCamPos;
        tempCam.transform.LookAt(transform.position);

        RenderTexture rt = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);
        tempCam.targetTexture = rt;

        Texture2D iconTex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        tempCam.Render();

        RenderTexture.active = rt;
        iconTex.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        iconTex.Apply();

        RenderTexture.active = null;
        tempCam.targetTexture = null;
        Destroy(rt);
        Destroy(camObj);

        Sprite sprite = Sprite.Create(iconTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100f);
        return sprite;
    }

    Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogWarning("⚠️ Sprite is null, using white texture");
            return Texture2D.whiteTexture;
        }

        Texture2D sourceTexture = sprite.texture;
        Rect rect = sprite.rect;

        int x = Mathf.FloorToInt(rect.x);
        int y = Mathf.FloorToInt(rect.y);
        int width = Mathf.FloorToInt(rect.width);
        int height = Mathf.FloorToInt(rect.height);

        if (sourceTexture.isReadable)
        {
            Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = sourceTexture.GetPixels(x, y, width, height);
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
        else
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(sourceTexture, rt);

            Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            newTexture.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return newTexture;
        }
    }

    public void SetInteractable(bool state)
    {
        enabled = state;
        if (!state && textDisplay != null)
        {
            textDisplay.gameObject.SetActive(false);
        }
    }

    public bool IsCollected()
    {
        return collected;
    }

    public void ResetItem()
    {
        collected = false;
        transform.position = startPosition;
        gameObject.SetActive(true);
        if (textDisplay != null) textDisplay.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);

        if (player != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
}