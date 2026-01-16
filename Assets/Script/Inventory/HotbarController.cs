using UnityEngine;

public class HotbarController : MonoBehaviour
{
    public int selectedSlot = 0;       // ช่องที่เลือกอยู่ (0-8)
    public RectTransform selector;    // ลากตัว 'Selector' มาใส่
    public Transform slotParent;      // ลากตัว 'Grid' ที่เก็บปุ่มมาใส่

    Inventory inventory;

    void Start()
    {
        inventory = Inventory.instance;
        UpdateSelectorUI();
    }

    void Update()
    {
        // 1. รับค่าจากปุ่มตัวเลข 1 - 9
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlot = i;
                UpdateSelectorUI();
                AutoEquip(); // สั่งให้ถือของในช่องที่เลือกทันที
            }
        }
    }

    void UpdateSelectorUI()
    {
        // ย้ายกรอบ Selector ไปบังช่องที่เลือก
        if (slotParent.childCount > selectedSlot)
        {
            Transform targetSlot = slotParent.GetChild(selectedSlot);
            selector.position = targetSlot.position;
        }
    }

    void AutoEquip()
    {
        // เช็คว่าในช่องที่เลือกมีของอยู่จริงไหม
        if (selectedSlot < inventory.items.Count)
        {
            inventory.EquipItem(inventory.items[selectedSlot].itemData);
        }
        else
        {
            inventory.Unequip(); // ถ้าช่องว่าง ให้เอามือเปล่า
        }
    }
}