using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootDrop
{
    public ItemData item;      // ไอเทมอะไร?
    [Range(0, 100)] public float dropChance; // โอกาสดรอป %
    public int minAmount = 1;  // จำนวนขั้นต่ำ
    public int maxAmount = 3;  // จำนวนสูงสุด
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootDrop> potentialLoot; // รายการของที่ "อาจจะ" ดรอป

    // ฟังก์ชันสำหรับสุ่มของออกมาจริงๆ
    public List<InventoryItem> GenerateLoot()
    {
        List<InventoryItem> loot = new List<InventoryItem>();

        foreach (var drop in potentialLoot)
        {
            // สุ่ม % (0-100) ถ้าผ่านเกณฑ์ก็ได้ของ
            if (Random.Range(0f, 100f) <= drop.dropChance)
            {
                int qty = Random.Range(drop.minAmount, drop.maxAmount + 1);
                loot.Add(new InventoryItem(drop.item, qty));
            }
        }
        return loot;
    }
}