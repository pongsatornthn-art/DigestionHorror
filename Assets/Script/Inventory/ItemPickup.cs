using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("ข้อมูลไอเท็ม")]
    public ItemData item;
    public float pickupRange = 3f;

    [Header("ตั้งค่ารูปภาพและ UI (Visuals)")]
    public GameObject normalObject;
    public GameObject hoverObject;
    public GameObject promptUI;

    // ⭐ ส่วนที่เพิ่มใหม่: ระยะเยื้องจากปลายเมาส์ (ปรับได้ใน Unity)
    public Vector3 promptOffset = new Vector3(0.5f, 0.5f, 0f);

    private Transform player;
    private bool isMouseOver = false;
    private Collider2D col;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        col = GetComponent<Collider2D>();
        UpdateVisuals(false);
    }

    void Update()
    {
        // หาตำแหน่งเมาส์ในโลกของเกม
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // ล็อกแกน Z ไว้กันภาพจม

        bool isHoveringNow = col.OverlapPoint(mouseWorldPos);

        if (isHoveringNow != isMouseOver)
        {
            isMouseOver = isHoveringNow;
            UpdateVisuals(isMouseOver);
        }

        // ⭐ ส่วนที่เพิ่มใหม่: บังคับให้ปุ่ม E วิ่งตามเมาส์ตลอดเวลาที่ชี้อยู่
        if (isMouseOver && promptUI != null)
        {
            promptUI.transform.position = mouseWorldPos + promptOffset;
        }

        if (isMouseOver && player != null)
        {
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
                        Debug.LogWarning("❌ เก็บไม่ได้! กระเป๋าเต็ม");
                    }
                }
            }
        }
    }

    void UpdateVisuals(bool isHovering)
    {
        if (normalObject != null) normalObject.SetActive(!isHovering);
        if (hoverObject != null) hoverObject.SetActive(isHovering);
        if (promptUI != null) promptUI.SetActive(isHovering);
    }
}