using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("🔦 Flashlight Controls")]
    public KeyCode toggleKey = KeyCode.F;
    public string itemID = "Flashlight";

    [Header("💡 Light Properties - Spotlight Style")]
    public float auraIntensity = 2.5f;
    public float auraRange = 15f; // เพิ่มระยะไกลขึ้นสำหรับ spotlight
    public Color auraColor = new Color(1f, 0.95f, 0.8f); // Warm light
    [Range(30f, 120f)]
    public float spotAngle = 60f; // มุมกว้างของแสง
    [Range(0f, 90f)]
    public float innerSpotAngle = 30f; // มุมแสงด้านใน
    [Range(0f, 2f)]
    public float flickerAmount = 0.05f; // ไฟกระพริบเล็กน้อย

    [Header("🔋 Battery System")]
    public bool useBattery = true;
    public float maxBatteryLife = 300f;
    public float batteryDrainRate = 1f;
    public float lowBatteryThreshold = 20f; // %

    [Header("📱 Visual Feedback")]
    public bool showFlashlightModel = true;
    public Vector3 flashlightPositionOffset = new Vector3(0.3f, -0.2f, 0.5f);
    public Vector3 flashlightRotationOffset = new Vector3(-10f, 0f, 0f);
    public float flashlightScale = 1f;

    [Header("🔊 Audio")]
    public AudioClip toggleOnSound;
    public AudioClip toggleOffSound;
    public AudioClip batteryLowWarning;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    // Private variables
    private Light auraLight;
    private float currentBattery;
    private bool isOn = false;
    private AudioSource audioSource;
    private RadialInventoryVertical inventory;
    private GameObject flashlightModel;
    private float flickerTimer = 0f;
    private float baseIntensity;
    private bool hasWarnedLowBattery = false;

    void Start()
    {
        InitializeFlashlight();
    }

    void InitializeFlashlight()
    {
        currentBattery = maxBatteryLife;
        baseIntensity = auraIntensity;
        inventory = FindObjectOfType<RadialInventoryVertical>();

        // สร้าง Spotlight สำหรับไฟฉาย
        GameObject lightObj = new GameObject("FlashlightSpot");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        // ✅ หมุนให้แสงชี้ไปด้านหน้า
        lightObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        auraLight = lightObj.AddComponent<Light>();

        // ✅ เปลี่ยนเป็น Spotlight
        auraLight.type = LightType.Spot;
        auraLight.intensity = auraIntensity;
        auraLight.range = auraRange;
        auraLight.color = auraColor;

        // ✅ ตั้งค่า Spotlight
        auraLight.spotAngle = spotAngle;
        auraLight.innerSpotAngle = innerSpotAngle;

        auraLight.shadows = LightShadows.Soft;
        auraLight.renderMode = LightRenderMode.ForcePixel;
        auraLight.enabled = false;

        // สร้าง AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = soundVolume;

        Debug.Log("🔦 Flashlight system initialized with spotlight!");
    }

    void Update()
    {
        bool holdingFlashlight = IsHoldingFlashlight();

        // แสดง/ซ่อนโมเดลไฟฉาย
        if (flashlightModel != null)
        {
            flashlightModel.SetActive(holdingFlashlight);
        }

        // ถ้าไม่ได้ถือไฟฉาย ปิดไฟ
        if (!holdingFlashlight)
        {
            if (isOn)
            {
                TurnOff();
            }
            return;
        }

        // Toggle ไฟฉาย
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }

        // ระบบแบตเตอรี่และเอฟเฟกต์
        if (isOn)
        {
            UpdateBattery();
            UpdateFlickerEffect();
        }
    }

    void UpdateBattery()
    {
        if (!useBattery) return;

        currentBattery -= batteryDrainRate * Time.deltaTime;

        if (currentBattery <= 0f)
        {
            currentBattery = 0f;
            TurnOff();
            Debug.Log("🔋 Battery depleted!");
            return;
        }

        float batteryPercent = GetBatteryPercent();

        // เตือนแบตต่ำ
        if (batteryPercent <= lowBatteryThreshold && !hasWarnedLowBattery)
        {
            hasWarnedLowBattery = true;
            if (batteryLowWarning != null)
            {
                audioSource.PlayOneShot(batteryLowWarning, soundVolume);
            }
            Debug.LogWarning("⚠️ Low battery!");
        }

        // ปรับความสว่างตามแบต
        if (batteryPercent < lowBatteryThreshold)
        {
            float dimFactor = Mathf.Lerp(0.3f, 1f, batteryPercent / lowBatteryThreshold);
            auraLight.intensity = baseIntensity * dimFactor;
        }
        else
        {
            auraLight.intensity = baseIntensity;
            hasWarnedLowBattery = false;
        }
    }

    void UpdateFlickerEffect()
    {
        if (flickerAmount <= 0f) return;

        flickerTimer += Time.deltaTime * Random.Range(8f, 12f);
        float flicker = Mathf.PerlinNoise(flickerTimer, 0f) * 2f - 1f;
        float currentIntensity = auraLight.intensity;
        auraLight.intensity = currentIntensity + (flicker * flickerAmount);
    }

    bool IsHoldingFlashlight()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<RadialInventoryVertical>();
            if (inventory == null) return false;
        }

        return inventory.HasItemInHand() && inventory.GetCurrentItemName() == itemID;
    }

    void ToggleFlashlight()
    {
        if (isOn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }

    void TurnOn()
    {
        if (useBattery && currentBattery <= 0f)
        {
            Debug.Log("⚠️ Battery is empty! Cannot turn on.");
            return;
        }

        isOn = true;
        auraLight.enabled = true;

        if (toggleOnSound != null)
        {
            audioSource.PlayOneShot(toggleOnSound, soundVolume);
        }

        Debug.Log("🔦 Flashlight ON - Spotlight mode");
    }

    void TurnOff()
    {
        isOn = false;
        auraLight.enabled = false;

        if (toggleOffSound != null)
        {
            audioSource.PlayOneShot(toggleOffSound, soundVolume);
        }

        Debug.Log("🔦 Flashlight OFF");
    }

    // Public Methods
    public void RechargeBattery(float amount)
    {
        if (!useBattery) return;

        float oldBattery = currentBattery;
        currentBattery = Mathf.Min(currentBattery + amount, maxBatteryLife);

        Debug.Log($"🔋 Battery recharged: {oldBattery:F1} → {currentBattery:F1} (+{amount:F1})");
    }

    public void SetBattery(float amount)
    {
        currentBattery = Mathf.Clamp(amount, 0f, maxBatteryLife);
    }

    public float GetBatteryPercent()
    {
        if (!useBattery) return 100f;
        return (currentBattery / maxBatteryLife) * 100f;
    }

    public float GetBatteryCurrent()
    {
        return currentBattery;
    }

    public bool IsFlashlightOn()
    {
        return isOn;
    }

    public void ForceToggle(bool state)
    {
        if (state) TurnOn();
        else TurnOff();
    }

    // UI Display
    void OnGUI()
    {
        if (!IsHoldingFlashlight()) return;

        int yOffset = 10;
        float percent = GetBatteryPercent();

        // Battery Display
        if (useBattery)
        {
            Color oldColor = GUI.color;

            if (percent < lowBatteryThreshold) GUI.color = Color.red;
            else if (percent < 50f) GUI.color = Color.yellow;
            else GUI.color = Color.green;

            GUI.Label(new Rect(10, yOffset, 300, 30),
                $"🔋 Battery: {percent:F0}% ({currentBattery:F1}s remaining)");
            yOffset += 30;

            GUI.color = oldColor;
        }

        // Status Display
        GUI.color = isOn ? Color.green : Color.gray;
        string status = isOn ? "🔦 ON" : "🔦 OFF";
        GUI.Label(new Rect(10, yOffset, 300, 30), $"{status} (Press {toggleKey} to toggle)");

        GUI.color = Color.white;
    }
}