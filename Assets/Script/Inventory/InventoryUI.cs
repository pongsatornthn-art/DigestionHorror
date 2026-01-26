using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    // ⭐ แก้ไขจุดที่ 1: เพิ่มตัวแปร instance
    public static InventoryUI instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform hotbarGrid;
    public Transform backpackGrid;

    Inventory inventory;
    InventorySlot[] hotbarSlots;
    InventorySlot[] backpackSlots;

    // ⭐ แก้ไขจุดที่ 2: Awake
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of InventoryUI found!");
            return;
        }
        instance = this;
    }

    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        hotbarSlots = hotbarGrid.GetComponentsInChildren<InventorySlot>();
        backpackSlots = backpackGrid.GetComponentsInChildren<InventorySlot>();

        UpdateUI();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    // ⭐ แก้ไขจุดที่ 3: Logic ปุ่ม I (สั่งปิดกล่องด้วย)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // เช็คว่า: มีระบบกล่องไหม? และหน้าต่างกล่องเปิดอยู่ไหม?
            if (ChestUI.instance != null && ChestUI.instance.chestPanel.activeSelf)
            {
                // ถ้ากล่องเปิดอยู่ -> สั่งปิดกล่อง (เดี๋ยวกระเป๋าจะปิดตามเอง)
                ChestUI.instance.CloseChest();
            }
            else
            {
                // ถ้ากล่องไม่ได้เปิด -> เปิด/ปิดกระเป๋าตามปกติ
                if (inventoryPanel != null)
                    inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            }
        }
    }

    void UpdateUI()
    {
        // ============================================
        // ส่วนที่ 1: จัดการ Hotbar
        // ============================================
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].slotIndex = i;

            if (i < inventory.items.Count && inventory.items[i] != null)
            {
                hotbarSlots[i].AddItem(inventory.items[i].itemData, inventory.items[i].amount, true);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }

        // ============================================
        // ส่วนที่ 2: จัดการ Backpack
        // ============================================
        for (int i = 0; i < backpackSlots.Length; i++)
        {
            int slotIdx = i + hotbarSlots.Length;

            backpackSlots[i].slotIndex = slotIdx;

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