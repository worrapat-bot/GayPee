using UnityEngine;

public class PlayerController58 : MonoBehaviour
{
    // [SerializeField] private float walkSpeed = 5f; // ตัวแปรความเร็ว (ถ้ายังใช้)
    [SerializeField] private CharacterController characterController; // ⬅️ ต้องเชื่อมต่อตัวนี้

    private bool canMove = true; // สถานะควบคุมการเดิน (เปิด/ปิด)

    private void Start()
    {
        // ตรวจสอบว่า CharacterController ถูกตั้งค่าแล้วหรือไม่
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!canMove) return; // 💡 NEW: ถ้า canMove เป็น false จะไม่ทำโค้ดการเดินต่อ

        // =======================================================
        // โค้ดการเคลื่อนไหว "ทั้งหมด" ของคุณควรอยู่ตรงนี้
        // เช่น Input.GetAxis, transform.Translate, หรือ .Move()
        // =======================================================

        // ตัวอย่างการเดินโดยใช้ CharacterController (ถ้าคุณใช้)
        /*
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * walkSpeed * Time.deltaTime);
        */
        
        // (ใส่โค้ดการเดินเดิมของคุณที่นี่)
    }

    /// <summary>
    /// Public Method ที่ EnemyPatrol จะเรียกใช้เพื่อสั่งหยุด
    /// </summary>
    /// <param name="state">true = เปิดการเดิน, false = ปิดการเดิน</param>
    public void SetMovementEnabled(bool state)
    {
        canMove = state;
    }
}