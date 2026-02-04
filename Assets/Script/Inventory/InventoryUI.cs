using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform hotbarGrid;
    public Transform backpackGrid;

    // ⭐ 1. เพิ่มตัวแปรรับกรอบขาว
    [Header("Selection System")]
    public RectTransform selectionCursor; // ลากรูปกรอบขาวมาใส่ตรงนี้

    Inventory inventory;
    InventorySlot[] hotbarSlots;
    InventorySlot[] backpackSlots;

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
        // เช็คก่อนว่ามี Event ให้เชื่อมไหม (ถ้าไม่มีก็ข้ามไป)
        if (inventory != null)
        {
            inventory.onItemChangedCallback += UpdateUI;
        }

        hotbarSlots = hotbarGrid.GetComponentsInChildren<InventorySlot>();
        backpackSlots = backpackGrid.GetComponentsInChildren<InventorySlot>();

        UpdateUI();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        // ⭐ 2. เริ่มเกมมา สั่งให้กรอบไปอยู่ที่ช่องแรกทันที
        SelectHotbarSlot(0);
    }

    void Update()
    {
        // Logic เปิด/ปิดกระเป๋าเดิมของคุณ
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (ChestUI.instance != null && ChestUI.instance.chestPanel.activeSelf)
            {
                ChestUI.instance.CloseChest();
            }
            else
            {
                if (inventoryPanel != null)
                    inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            }
        }

        // ⭐ 3. เพิ่มฟังก์ชันเช็คปุ่มตัวเลข
        HandleHotbarInput();
    }

    void UpdateUI()
    {
        // ============================================
        // ส่วนจัดการ Hotbar (เดิม)
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
        // ส่วนจัดการ Backpack (เดิม)
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

    // ============================================
    // ⭐ 4. เพิ่มฟังก์ชันใหม่: เช็คปุ่มกด 1-5
    // ============================================
    void HandleHotbarInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectHotbarSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectHotbarSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectHotbarSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectHotbarSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectHotbarSlot(4);
    }

    // ============================================
    // ⭐ 5. เพิ่มฟังก์ชันใหม่: สั่งย้ายกรอบ
    // ============================================
    void SelectHotbarSlot(int index)
    {
        // ป้องกัน Error ถ้าช่องยังไม่โหลด
        if (hotbarSlots == null || index < 0 || index >= hotbarSlots.Length) return;

        // สั่งย้ายกรอบขาว ไปที่ตำแหน่งของช่องเป้าหมาย
        if (selectionCursor != null && hotbarSlots[index] != null)
        {
            selectionCursor.position = hotbarSlots[index].transform.position;
            // Debug.Log("เลือกช่อง: " + (index + 1));
        }
    }
}