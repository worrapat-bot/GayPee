// CameraWobble.cs (Updated)
using UnityEngine;

public class CameraWobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    [Tooltip("ความถี่ของการสั่น/หมุน (ยิ่งมาก ยิ่งเร็ว)")]
    [SerializeField]
    private float wobbleSpeed = 0.5f; 

    // เพิ่มตัวแปรเปิด/ปิดการสั่นของตำแหน่ง
    [Header("Position Wobble")]
    [Tooltip("เปิด/ปิดการสั่นของตำแหน่งกล้อง")]
    public bool enablePositionWobble = true; 
    [Tooltip("ความแรงของการสั่นของตำแหน่ง (ยิ่งมาก ยิ่งสั่นแรง)")]
    [SerializeField]
    private float positionIntensity = 0.1f; 

    // เพิ่มตัวแปรเปิด/ปิดการหมุนวน
    [Header("Rotation Wobble")]
    [Tooltip("เปิด/ปิดการหมุนวนของกล้อง")]
    public bool enableRotationWobble = true; 
    [Tooltip("ความแรงของการหมุน (Roll/Pitch/Yaw)")]
    [SerializeField]
    private float rotationIntensity = 0.5f;

    // ตัวแปรสำหรับควบคุมการสุ่ม (Perlin Noise)
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;

    // ตำแหน่งเริ่มต้นของกล้อง
    private Vector3 initialPosition;
    // การหมุนเริ่มต้นของกล้อง
    private Quaternion initialRotation;

    void Start()
    {
        // 1. บันทึกตำแหน่งและมุมหมุนเริ่มต้นไว้
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        
        // 2. กำหนดจุดเริ่มต้นการสุ่ม Perlin Noise
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
        noiseOffsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        ApplyWobble();
    }

    private void ApplyWobble()
    {
        float time = Time.time * wobbleSpeed;
        Vector3 currentPosition = initialPosition;
        Quaternion currentRotation = initialRotation;

        // ----------------------------------------------------
        // A. การสั่นของตำแหน่ง (Position Wobble)
        // ----------------------------------------------------
        if (enablePositionWobble) // ใช้ตัวแปรที่เราเพิ่มมาเช็ค
        {
            float x = (Mathf.PerlinNoise(noiseOffsetX + time, 0f) - 0.5f) * positionIntensity;
            float y = (Mathf.PerlinNoise(noiseOffsetY + time, 0f) - 0.5f) * positionIntensity;
            float z = (Mathf.PerlinNoise(noiseOffsetZ + time, 0f) - 0.5f) * positionIntensity;

            currentPosition += new Vector3(x, y, z);
        }
        
        // ----------------------------------------------------
        // B. การหมุนวน (Rotation Wobble)
        // ----------------------------------------------------
        if (enableRotationWobble) // ใช้ตัวแปรที่เราเพิ่มมาเช็ค
        {
            // Roll (Z-axis): การเอียงหัวไปด้านข้าง
            float roll = (Mathf.PerlinNoise(noiseOffsetX + time + 10f, 0f) - 0.5f) * rotationIntensity;
            
            // Pitch (X-axis): การพยักหน้า/เงยหน้า
            float pitch = (Mathf.PerlinNoise(noiseOffsetY + time + 20f, 0f) - 0.5f) * rotationIntensity;
            
            // Yaw (Y-axis): การส่ายหัวไปด้านข้าง (ปรับให้เบาลง)
            float yaw = (Mathf.PerlinNoise(noiseOffsetZ + time + 30f, 0f) - 0.5f) * (rotationIntensity * 0.5f);
            
            // รวมการหมุนวนเข้ากับการหมุนเริ่มต้น
            Quaternion wobbleRotation = Quaternion.Euler(pitch, yaw, roll); 
            currentRotation *= wobbleRotation; // ใช้คูณเพื่อรวมการหมุน
        }

        // นำค่าไปใช้กับ Transform
        transform.localPosition = currentPosition;
        transform.localRotation = currentRotation;
    }
}