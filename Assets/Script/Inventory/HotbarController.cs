using UnityEngine;

public class HotbarController : MonoBehaviour
{
    Inventory inventory;
    int currentSlotIndex = 0;

    void Start()
    {
        inventory = Inventory.instance;

        // สั่งให้รีเฟรชมือทันทีที่มีของเข้า/ออกกระเป๋า
        inventory.onItemChangedCallback += RefreshHand;
        RefreshHand();
    }

    void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentSlotIndex = i;
                RefreshHand(); // ใช้ฟังก์ชันเดียวกันจะได้ไม่งง
            }
        }
    }

    void RefreshHand()
    {
        // ป้องกัน Error กรณีของยังไม่โหลด
        if (inventory == null || inventory.items == null || currentSlotIndex >= inventory.items.Count) return;

        var itemSlot = inventory.items[currentSlotIndex];

        if (itemSlot != null) // ถ้าช่องนี้มีของ
        {
            ItemData itemData = itemSlot.itemData;

            // 1. บอก Inventory (ระบบข้อมูล)
            inventory.EquipItem(itemData);

            // ⭐ 2. บอก PlayerController (ระบบเปลี่ยนร่าง) <-- บรรทัดนี้สำคัญมากที่ขาดไป!
            if (PlayerController.instance != null)
            {
                PlayerController.instance.EquipWeapon(itemData);
            }
        }
        else // ถ้าช่องนี้ว่างเปล่า
        {
            // 1. บอก Inventory ให้เอามือลง
            inventory.Unequip();

            // ⭐ 2. บอก PlayerController ให้กลับร่างเดิม
            if (PlayerController.instance != null)
            {
                PlayerController.instance.EquipWeapon(null);
            }
        }
    }
}