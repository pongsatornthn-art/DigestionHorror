using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("ข้อมูลไอเท็ม")]
    public ItemData item;             // ข้อมูลไอเท็มที่จะเข้ากระเป๋า
    public float pickupRange = 3f;    // ระยะเก็บ

    [Header("ตั้งค่ารูปภาพ (Visuals)")]
    public GameObject normalObject;   // ลากตัวลูก: รูปปกติ
    public GameObject hoverObject;    // ลากตัวลูก: รูปตอนเมาส์ชี้

    private Transform player;

    void Start()
    {
        // หาตัว Player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // เริ่มเกม: เปิดตัวปกติ, ปิดตัวตอนชี้
        UpdateVisuals(false);
    }

    void OnMouseEnter()
    {
        // เมาส์เข้า -> โชว์รูป Hover
        UpdateVisuals(true);
    }

    void OnMouseExit()
    {
        // เมาส์ออก -> โชว์รูปปกติ
        UpdateVisuals(false);
    }

    void OnMouseOver()
    {
        if (player == null) return;

        // เช็คระยะห่าง
        if (Vector2.Distance(transform.position, player.position) <= pickupRange)
        {
            // ถ้าอยู่ในระยะและกด E
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Inventory.instance.AddItem(item))
                {
                    Debug.Log("เก็บของ: " + item.itemName);
                    Destroy(gameObject); // ทำลายตัวเองทิ้ง
                }
            }
        }
    }

    // ฟังก์ชันช่วยสลับรูป (จะได้ไม่ต้องเขียนซ้ำ)
    void UpdateVisuals(bool isHovering)
    {
        if (normalObject != null) normalObject.SetActive(!isHovering);
        if (hoverObject != null) hoverObject.SetActive(isHovering);
    }
}