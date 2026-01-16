using UnityEngine;

public class HotbarController : MonoBehaviour
{
    Inventory inventory;

    // ⭐ แก้ที่ 1: ตั้งค่าเริ่มต้นเป็น 0 (ช่องแรก) เสมอ
    int currentSlotIndex = 0;

    void Start()
    {
        inventory = Inventory.instance;

        // สั่งให้คอยฟังการเปลี่ยนแปลงในกระเป๋า
        inventory.onItemChangedCallback += RefreshHand;

        // ⭐ แก้ที่ 2: เริ่มเกมปุ๊บ สั่งให้เช็คของในมือทันที (จะได้ไม่บั๊กตอนเริ่ม)
        RefreshHand();
        Debug.Log("เริ่มเกม: เลือกช่อง 1 อัตโนมัติ");
    }

    void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentSlotIndex = i;
                EquipBySlotIndex(i);
                // Debug.Log($"กดปุ่มเลือกช่อง: {i}");
            }
        }
    }

    // ฟังก์ชันรีเฟรชมือ (เรียกเมื่อของในกระเป๋าเปลี่ยน)
    void RefreshHand()
    {
        // Debug.Log("มีการขยับของในกระเป๋า! กำลังเช็คของในมือ..."); 

        if (currentSlotIndex >= 0)
        {
            EquipBySlotIndex(currentSlotIndex);
        }
    }

    void EquipBySlotIndex(int index)
    {
        // ป้องกัน Error กรณี Index เกินจำนวนช่อง
        if (index >= inventory.items.Count) return;

        // เช็คว่าช่องนั้นมีของ และไม่ใช่ช่องว่าง (null)
        if (inventory.items[index] != null)
        {
            // มีของ -> สั่งถือ
            inventory.EquipItem(inventory.items[index].itemData);
        }
        else
        {
            // ของหาย / เป็นช่องว่าง -> เก็บมือ
            inventory.Unequip();
            // Debug.Log($"ช่อง {index} ว่างเปล่า -> เก็บมือ");
        }
    }
}