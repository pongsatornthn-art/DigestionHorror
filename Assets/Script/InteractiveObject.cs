using UnityEngine;
// ไม่ต้องใช้ TMPro แล้ว เพราะเราลบส่วนข้อความออก

public class InteractiveObject : MonoBehaviour
{
    [Header("ตั้งค่ารูปภาพ (ลากตัวลูกมาใส่)")]
    public GameObject normalObject; // รูปปกติ
    public GameObject hoverObject;  // รูปตอนเมาส์ชี้

    // --- ส่วนตั้งค่าชื่อถูกลบออกแล้ว ---

    void Start()
    {
        // เริ่มเกม: เปิดตัวปกติ, ปิดตัวตอนชี้
        if (normalObject != null) normalObject.SetActive(true);
        if (hoverObject != null) hoverObject.SetActive(false);
    }

    void OnMouseEnter()
    {
        // เมาส์เข้า: สลับเป็นตัวโชว์
        if (normalObject != null) normalObject.SetActive(false);
        if (hoverObject != null) hoverObject.SetActive(true);
    }

    void OnMouseExit()
    {
        // เมาส์ออก: สลับกลับเป็นตัวปกติ
        if (normalObject != null) normalObject.SetActive(true);
        if (hoverObject != null) hoverObject.SetActive(false);
    }
}