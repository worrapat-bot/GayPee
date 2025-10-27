using UnityEngine;

public class FlashlightGenerator : MonoBehaviour
{
    public Light flashlight;             // ใส่ไฟฉายใน Inspector
    public float chargeRate = 5f;        // อัตราการชาร์จเมื่อหมุน
    public float drainRate = 1f;         // อัตราการลดพลัง
    public float maxPower = 100f;        // พลังสูงสุด
    public float rotationThreshold = 0.1f;  // ความแรงที่ถือว่าหมุน

    private float currentPower = 50f;
    private Vector3 lastRotation;

    void Start()
    {
        if (flashlight != null)
            flashlight.intensity = currentPower / maxPower;
        lastRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        // ตรวจจับการหมุน
        Vector3 deltaRotation = transform.rotation.eulerAngles - lastRotation;
        float rotationMagnitude = deltaRotation.magnitude;

        // ถ้ามีการหมุนเร็วพอ → ชาร์จพลังงาน
        if (rotationMagnitude > rotationThreshold)
            currentPower += chargeRate * Time.deltaTime;
        else
            currentPower -= drainRate * Time.deltaTime;

        currentPower = Mathf.Clamp(currentPower, 0f, maxPower);

        // ปรับความสว่างไฟฉาย
        if (flashlight != null)
            flashlight.intensity = Mathf.Lerp(flashlight.intensity, currentPower / maxPower, 10f * Time.deltaTime);

        lastRotation = transform.rotation.eulerAngles;
    }
}
