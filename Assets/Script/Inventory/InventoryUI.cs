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

        hotbarSlots = hotbarGrid.GetComponentsInChildren<InventorySlot>();
        backpackSlots = backpackGrid.GetComponentsInChildren<InventorySlot>();

        UpdateUI();
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
        // 1. Hotbar Loop
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].slotIndex = i;

            // ⭐ เช็คเพิ่มว่า: ช่องนี้มีข้อมูล และ "ไม่ใช่ null"
            if (i < inventory.items.Count && inventory.items[i] != null)
            {
                hotbarSlots[i].AddItem(inventory.items[i].itemData, inventory.items[i].amount, true);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }

        // 2. Backpack Loop
        for (int i = 0; i < backpackSlots.Length; i++)
        {
            int slotIdx = i + hotbarSlots.Length;
            backpackSlots[i].slotIndex = slotIdx;

            // ⭐ เช็ค null เหมือนกัน
            if (slotIdx < inventory.items.Count && inventory.items[slotIdx] != null)
            {
                backpackSlots[i].AddItem(inventory.items[slotIdx].itemData, inventory.items[slotIdx].amount, false);
            }
            else
            {
                backpackSlots[i].ClearSlot();
            }
        }
    }
}