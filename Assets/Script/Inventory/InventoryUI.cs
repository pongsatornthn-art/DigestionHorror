using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;

    [Header("Slot Parents")]
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
            if (inventoryPanel != null)
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    void UpdateUI()
    {
        // 1. อัปเดต Hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            // ⭐ บอกเลขที่แท้จริง
            hotbarSlots[i].slotIndex = i;

            if (i < inventory.items.Count)
            {
                hotbarSlots[i].AddItem(inventory.items[i].itemData, inventory.items[i].amount, true);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }

        // 2. อัปเดต Backpack
        for (int i = 0; i < backpackSlots.Length; i++)
        {
            int inventoryIndex = i + hotbarSlots.Length; // คำนวณเลขที่แท้จริง

            // ⭐ บอกเลขที่แท้จริง
            backpackSlots[i].slotIndex = inventoryIndex;

            if (inventoryIndex < inventory.items.Count)
            {
                backpackSlots[i].AddItem(inventory.items[inventoryIndex].itemData, inventory.items[inventoryIndex].amount, false);
            }
            else
            {
                backpackSlots[i].ClearSlot();
            }
        }
    }
}