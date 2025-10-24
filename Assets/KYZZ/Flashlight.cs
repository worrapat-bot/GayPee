using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("🔦 Flashlight Settings")]
    public KeyCode toggleKey = KeyCode.F;
    public string itemID = "Flashlight"; // ใช้ตรวจใน Inventory

    [Header("Light Properties")]
    public float lightIntensity = 3f;
    public float lightRange = 15f;
    public float spotAngle = 60f;
    public Color lightColor = Color.white;

    [Header("Battery System (Optional)")]
    public bool useBattery = false;
    public float batteryLife = 300f;
    public float batteryDrainRate = 1f;

    [Header("🔊 Sounds")]
    public AudioClip clickSound;

    private Light flashlight;
    private float currentBattery;
    private bool isOn = false;
    private AudioSource audioSource;
    private RadialInventoryVertical inventory;

    void Start()
    {
        currentBattery = batteryLife;
        inventory = FindObjectOfType<RadialInventoryVertical>();

        // สร้าง Light
        GameObject lightObj = new GameObject("FlashlightBeam");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        lightObj.transform.localRotation = Quaternion.identity;

        flashlight = lightObj.AddComponent<Light>();
        flashlight.type = LightType.Spot;
        flashlight.intensity = lightIntensity;
        flashlight.range = lightRange;
        flashlight.spotAngle = spotAngle;
        flashlight.color = lightColor;
        flashlight.shadows = LightShadows.Soft;
        flashlight.enabled = false;

        // สร้าง AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        Debug.Log("🔦 Flashlight created!");
    }

    void Update()
    {
        // ต้องถือไฟฉายถึงจะใช้ได้
        if (!IsHoldingFlashlight())
        {
            if (isOn)
            {
                isOn = false;
                flashlight.enabled = false;
            }
            return;
        }

        // กด F เปิด/ปิดไฟฉาย
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }

        // ระบบแบตเตอรี่
        if (useBattery && isOn)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;

            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                isOn = false;
                flashlight.enabled = false;
                Debug.Log("🔋 Battery dead!");
            }

            // ไฟริบหรี่เมื่อแบตใกล้หมด
            if (currentBattery < batteryLife * 0.2f)
            {
                flashlight.intensity = Mathf.Lerp(0.5f, lightIntensity, currentBattery / (batteryLife * 0.2f));
            }
            else
            {
                flashlight.intensity = lightIntensity;
            }
        }
    }

    bool IsHoldingFlashlight()
    {
        if (inventory == null) return false;

        return inventory.HasItemInHand() &&
               inventory.GetCurrentItemName() == itemID;
    }

    void ToggleFlashlight()
    {
        if (useBattery && currentBattery <= 0f)
        {
            Debug.Log("⚠️ Battery empty!");
            return;
        }

        isOn = !isOn;
        flashlight.enabled = isOn;

        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, 0.5f);
        }

        Debug.Log(isOn ? "🔦 Flashlight ON" : "🔦 Flashlight OFF");
    }

    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, batteryLife);
        Debug.Log($"🔋 Battery recharged! Current: {currentBattery:F1}/{batteryLife}");
    }

    public float GetBatteryPercent()
    {
        return (currentBattery / batteryLife) * 100f;
    }

    void OnGUI()
    {
        if (useBattery && IsHoldingFlashlight())
        {
            float percent = GetBatteryPercent();
            Color oldColor = GUI.color;

            if (percent < 20f) GUI.color = Color.red;
            else if (percent < 50f) GUI.color = Color.yellow;
            else GUI.color = Color.green;

            GUI.Label(new Rect(10, 10, 250, 30), $"🔋 Battery: {percent:F0}%");
            GUI.Label(new Rect(10, 40, 250, 30), isOn ? "🔦 ON (Press F to turn off)" : "🔦 OFF (Press F to turn on)");

            GUI.color = oldColor;
        }
    }
}
