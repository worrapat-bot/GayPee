using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HauntedLightDeluxe : MonoBehaviour
{
    [Header("💡 Light Settings")]
    public Light lightSource;
    [Tooltip("ความสว่างต่ำสุดและสูงสุดของไฟ")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.5f;
    [Tooltip("ช่วงเวลาการกระพริบ (สุ่ม)")]
    public float minFlickerTime = 0.05f;
    public float maxFlickerTime = 0.2f;

    [Header("⚡ Outage Settings")]
    [Tooltip("โอกาสไฟจะดับยาว (0 = ไม่มี, 1 = ดับตลอด)")]
    [Range(0f, 1f)] public float outageChance = 0.05f;
    [Tooltip("ระยะเวลาที่ไฟดับนาน (วินาที)")]
    public float outageDuration = 2f;

    [Header("🔊 Sound Settings")]
    public AudioSource flickerSound;
    public AudioClip flickerClip;
    [Range(0f, 1f)] public float flickerVolume = 0.8f;
    [Tooltip("ระยะใกล้สุดที่เสียงจะดังเต็มที่")]
    public float minHearDistance = 3f;
    [Tooltip("ระยะไกลสุดที่ยังได้ยินเสียงเบาๆ")]
    public float maxHearDistance = 15f;

    private float timer;
    private bool isOutage = false;
    private float outageTimer = 0f;

    void Start()
    {
        // ✅ ตรวจสอบ Light
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>();
            if (lightSource == null)
                Debug.LogWarning("HauntedLightDeluxe: ไม่มี Light อยู่บนวัตถุนี้!");
        }

        // ✅ ตรวจสอบ Audio
        if (flickerSound == null)
            flickerSound = GetComponent<AudioSource>();

        // ตั้งค่าเสียง 3D ให้มีระยะ
        flickerSound.spatialBlend = 1f;       // 1 = 3D Sound
        flickerSound.minDistance = minHearDistance;
        flickerSound.maxDistance = maxHearDistance;
        flickerSound.volume = flickerVolume;
        flickerSound.loop = false;
        flickerSound.playOnAwake = false;

        ChangeLight();
    }

    void Update()
    {
        if (isOutage)
        {
            outageTimer -= Time.deltaTime;
            if (outageTimer <= 0)
            {
                // 🔁 ไฟกลับมาติด
                isOutage = false;
                lightSource.enabled = true;
                ChangeLight();
            }
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
            ChangeLight();
    }

    void ChangeLight()
    {
        // ⚡ โอกาสไฟดับยาว
        if (Random.value < outageChance)
        {
            StartOutage();
            return;
        }

        // 💡 กระพริบความสว่างแบบสุ่ม
        if (lightSource != null)
            lightSource.intensity = Random.Range(minIntensity, maxIntensity);

        // 🔊 เล่นเสียงไฟซ่าบางจังหวะ
        if (flickerClip && Random.value > 0.7f)
            flickerSound.PlayOneShot(flickerClip, flickerVolume);

        // ตั้งเวลาสุ่มรอบต่อไป
        timer = Random.Range(minFlickerTime, maxFlickerTime);
    }

    void StartOutage()
    {
        isOutage = true;
        outageTimer = outageDuration;
        if (lightSource != null)
            lightSource.enabled = false;

        // 🔊 เสียงไฟช็อตตอนดับ
        if (flickerClip)
            flickerSound.PlayOneShot(flickerClip, flickerVolume);
    }
}