using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform hotbarGrid;
    public Transform backpackGrid;

    Inventory inventory;
    InventorySlot[] hotbarSlots;
    InventorySlot[] backpackSlots;

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        // ดึง Slot ทั้งหมดมาจากลูกของ Grid
        hotbarSlots = hotbarGrid.GetComponentsInChildren<InventorySlot>();
        backpackSlots = backpackGrid.GetComponentsInChildren<InventorySlot>();

        UpdateUI();

        // เริ่มเกมมาให้ปิดกระเป๋าก่อน
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    void UpdateUI()
    {
        // ============================================
        // ส่วนที่ 1: จัดการ Hotbar (ช่อง 0 ถึง 9)
        // ============================================
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].slotIndex = i;

            // เช็คว่ามีของใน Data และต้องไม่เป็นช่องว่าง (null)
            if (i < inventory.items.Count && inventory.items[i] != null)
            {
                // ⭐ ส่ง "จำนวน" (amount) ไปด้วย เพื่อให้เลขขึ้น
                hotbarSlots[i].AddItem(inventory.items[i].itemData, inventory.items[i].amount, true);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }

        // ============================================
        // ส่วนที่ 2: จัดการ Backpack (ช่อง 10 ขึ้นไป)
        // ============================================
        for (int i = 0; i < backpackSlots.Length; i++)
        {
            // คำนวณ Index จริง: เริ่มต่อจาก Hotbar
            int slotIdx = i + hotbarSlots.Length;

            backpackSlots[i].slotIndex = slotIdx;

            // เช็คว่ามีของใน Data และต้องไม่เป็นช่องว่าง (null)
            if (slotIdx < inventory.items.Count && inventory.items[slotIdx] != null)
            {
                // ⭐ ส่ง "จำนวน" (amount) ไปด้วย
                backpackSlots[i].AddItem(inventory.items[slotIdx].itemData, inventory.items[slotIdx].amount, false);
            }
            else
            {
                backpackSlots[i].ClearSlot();
            }
        }
    }
}