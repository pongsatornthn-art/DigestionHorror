using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting; // ⭐ เพิ่มบรรทัดนี้

[Preserve] // ⭐ สั่งให้ Unity ห้ามลบคลาสนี้ตอน Build!
[System.Serializable]
public class LootDrop
{
    public ItemData item;
    [Range(0, 100)] public float dropChance;
    public int minAmount = 1;
    public int maxAmount = 3;
}

[Preserve] // ⭐ สั่งให้ Unity ห้ามลบคลาสนี้ตอน Build!
[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootDrop> potentialLoot;

    public List<InventoryItem> GenerateLoot()
    {
        List<InventoryItem> loot = new List<InventoryItem>();

        foreach (var drop in potentialLoot)
        {
            if (Random.Range(0f, 100f) <= drop.dropChance)
            {
                int qty = Random.Range(drop.minAmount, drop.maxAmount + 1);
                loot.Add(new InventoryItem(drop.item, qty));
            }
        }
        return loot;
    }
}