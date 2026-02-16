using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform hotbarGrid;
    public Transform backpackGrid;
    public GameObject craftingPanel;

    [Header("Selection System")]
    public RectTransform selectionCursor;

    Inventory inventory;
    InventorySlot[] hotbarSlots;
    InventorySlot[] backpackSlots;

    void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    void Start()
    {
        inventory = Inventory.instance;
        if (inventory != null) inventory.onItemChangedCallback += UpdateUI;

        hotbarSlots = hotbarGrid.GetComponentsInChildren<InventorySlot>();
        backpackSlots = backpackGrid.GetComponentsInChildren<InventorySlot>();

        UpdateUI();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        SelectHotbarSlot(0);

        // ✅ เริ่มเกมมาสั่งให้เมาส์ "โชว์" ทันที (แก้จาก false เป็น true)
        SetMouseState(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // ✅ บังคับให้เมาส์โชว์ตลอดเวลาในทุกเฟรม (กันเหนียว)
        if (!Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined; // ให้อยู่ในกรอบหน้าต่างเกม
        }

        // ถ้าเปิดกระเป๋าอยู่ ปลดล็อกให้ขยับอิสระ
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        HandleHotbarInput();
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        if (craftingPanel != null) craftingPanel.SetActive(isActive);

        SetMouseState(isActive);

        // คืนของอัตโนมัติถ้าปิดหน้าต่าง
        if (!isActive && CraftingManager.instance != null)
        {
            CraftingManager.instance.CancelCrafting();
        }
    }

    void SetMouseState(bool isUIOpen)
    {
        // ✅ ไม่ว่าจะเปิดหรือปิด UI ให้เมาส์โชว์ตลอด (Cursor.visible = true)
        Cursor.visible = true;

        if (isUIOpen)
        {
            Cursor.lockState = CursorLockMode.None; // เปิดเป๋า: เมาส์อิสระ
        }
        else
        {
            // ปิดเป๋า (ตอนเล่น): ให้เมาส์โชว์ แต่ล็อกไม่ให้หลุดออกนอกจอเกม
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    // --- ส่วน UpdateUI และ HandleHotbarInput คงเดิม ---
    void UpdateUI()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].slotIndex = i;
            if (i < inventory.items.Count && inventory.items[i] != null)
                hotbarSlots[i].AddItem(inventory.items[i].itemData, inventory.items[i].amount, true);
            else
                hotbarSlots[i].ClearSlot();
        }

        for (int i = 0; i < backpackSlots.Length; i++)
        {
            int slotIdx = i + hotbarSlots.Length;
            backpackSlots[i].slotIndex = slotIdx;
            if (slotIdx < inventory.items.Count && inventory.items[slotIdx] != null)
                backpackSlots[i].AddItem(inventory.items[slotIdx].itemData, inventory.items[slotIdx].amount, false);
            else
                backpackSlots[i].ClearSlot();
        }
    }

    void HandleHotbarInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectHotbarSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectHotbarSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectHotbarSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectHotbarSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectHotbarSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectHotbarSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectHotbarSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectHotbarSlot(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SelectHotbarSlot(8);
    }

    void SelectHotbarSlot(int index)
    {
        if (hotbarSlots == null || index < 0 || index >= hotbarSlots.Length) return;
        if (selectionCursor != null && hotbarSlots[index] != null)
            selectionCursor.position = hotbarSlots[index].transform.position;

        if (index < inventory.items.Count && inventory.items[index] != null)
        {
            ItemData itemToEquip = inventory.items[index].itemData;
            if (Inventory.instance != null) Inventory.instance.EquipItem(itemToEquip);
            if (PlayerController.instance != null) PlayerController.instance.EquipWeapon(itemToEquip);
        }
        else
        {
            if (Inventory.instance != null) Inventory.instance.Unequip();
            if (PlayerController.instance != null) PlayerController.instance.EquipWeapon(null);
        }
    }
}