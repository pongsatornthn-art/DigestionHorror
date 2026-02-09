using UnityEngine;

// สร้างหมวดหมู่ไอเทม
public enum ItemType
{
    General,
    Weapon,
    Totem,
    Consumable
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon;

    [TextArea] public string description;

    // ⭐ ตัวนี้ที่เคยหายไปครับ ผมเติมให้แล้ว
    public Sprite descriptionImage;

    public bool isStackable = true;
    public int maxStack = 99;
    public ItemType itemType;

    [Header("Equipment Visuals")]
    public Sprite equippedSprite; // รูปตอนถือ

    [Header("Combat Stats (Only for Weapons)")]
    public int weaponID; // 0=มีด, 1=ดาบ, 2=ขวาน

    [Header("Light Attack")]
    public int damage = 10;
    public float staminaCost = 10f;
    public float knockback = 3f;

    [Header("Heavy Attack")]
    public int heavyDamage = 20;
    public float heavyStaminaCost = 25f;
    public float heavyKnockback = 6f;
}