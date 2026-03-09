using UnityEngine;

[RequireComponent(typeof(Collider2D))] // บังคับว่าต้องมี Collider 2D เสมอ
public class ItemPickup : MonoBehaviour
{
    [Header("ข้อมูลไอเท็ม")]
    public ItemData item;
    public float pickupRange = 3f;

    [Header("ตั้งค่ารูปภาพ (Visuals)")]
    public GameObject normalObject;
    public GameObject hoverObject;

    private Transform player;
    private bool isMouseOver = false;
    private Collider2D col; // ตัวแปรเก็บกรอบฟิสิกส์ของไอเทม

    void Start()
    {
        // หาตัว Player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // ดึงคอมโพเนนต์ Collider มาเตรียมไว้ใช้งาน
        col = GetComponent<Collider2D>();

        // เริ่มเกม: เปิดตัวปกติ, ปิดตัวตอนชี้
        UpdateVisuals(false);
    }

    void Update()
    {
        // 1. แปลงตำแหน่งเมาส์บนหน้าจอ ให้เป็นตำแหน่งในโลกของเกม
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. เช็คว่าเมาส์จิ้มโดนกรอบฟิสิกส์ของไอเทมชิ้นนี้อยู่ไหม (แม่นยำและไม่จำศีลแน่นอน)
        bool isHoveringNow = col.OverlapPoint(mouseWorldPos);

        // 3. ถ้าสถานะการชี้เปลี่ยนไป (เพิ่งชี้ หรือ เพิ่งเอาเมาส์ออก) ให้สลับรูปภาพ
        if (isHoveringNow != isMouseOver)
        {
            isMouseOver = isHoveringNow;
            UpdateVisuals(isMouseOver);
        }

        // 4. ระบบกดปุ่มเก็บของ
        if (isMouseOver && player != null)
        {
            // เช็คระยะห่างระหว่างตัวละครกับไอเทม
            if (Vector2.Distance(transform.position, player.position) <= pickupRange)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (Inventory.instance != null && Inventory.instance.AddItem(item))
                    {
                        Debug.Log("เก็บของสำเร็จ: " + item.itemName);
                        Destroy(gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("❌ เก็บไม่ได้! กระเป๋าเต็ม หรือระบบ Inventory มีปัญหา");
                    }
                }
            }
        }
    }

    // ฟังก์ชันช่วยสลับรูปภาพ
    void UpdateVisuals(bool isHovering)
    {
        if (normalObject != null) normalObject.SetActive(!isHovering);
        if (hoverObject != null) hoverObject.SetActive(isHovering);
    }
}