using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent;   // ลาก Grid มาใส่ตรงนี้
    public GameObject inventoryUI;  // ลากหน้าต่าง UI มาใส่ตรงนี้ (เพื่อเปิด/ปิด)

    Inventory inventory;
    InventorySlot[] slots;

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        // ดึงปุ่มลูกๆ ทั้งหมดมาเก็บไว้ในลิสต์
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        // ✅ [สำคัญ] สั่งให้อัปเดตหน้าจอทันที 1 ครั้งตอนเริ่มเกม
        // เพื่อให้ของที่เราใส่ไว้ใน Inspector มันโชว์ขึ้นมาเลย
        UpdateUI();
    }

    void Update()
    {
        // กดปุ่ม I เพื่อเปิด/ปิดกระเป๋า
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                // ถ้ามีของ -> เอารูปมาใส่
                slots[i].AddItem(inventory.items[i]);
            }
            else
            {
                // ถ้าไม่มีของ -> เคลียร์ช่องให้ว่าง
                slots[i].ClearSlot();
            }
        }
    }
}