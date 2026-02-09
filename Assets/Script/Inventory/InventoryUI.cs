using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform hotbarGrid;
    public Transform backpackGrid;

    [Header("Selection System")]
    public RectTransform selectionCursor;

    Inventory inventory;
    InventorySlot[] hotbarSlots;
    InventorySlot[] backpackSlots;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one InventoryUI found!");
            return;
        }
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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (ChestUI.instance != null && ChestUI.instance.chestPanel.activeSelf)
                ChestUI.instance.CloseChest();
            else
                if (inventoryPanel != null) inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }

        HandleHotbarInput();
    }

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