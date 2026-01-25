using UnityEngine;

// 1. สร้างหัวข้อหมวดหมู่ไว้ข้างนอก Class (เพื่อให้คนอื่นเรียกใช้ได้)
public enum ItemType
{
    General,    // ของทั่วไป (ไม้, หิน, ขยะ)
    Weapon,     // อาวุธ (ดาบ, ขวาน)
    Totem,      // โทเทม (เครื่องราง)
    Consumable  // (แถมให้) ของกินได้ เช่น ยา
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon;

    [TextArea] public string description;
    public Sprite descriptionImage;

    public bool isStackable = true;
    public int maxStack = 99;

    // ⭐ 2. เพิ่มตัวแปรนี้เข้าไป เพื่อให้เลือกหมวดหมู่ได้ใน Unity Inspector
    public ItemType itemType;

    [Header("Equipment Settings")]
    public Sprite equippedSprite; // รูปตอนถือ
    public int damage = 0;        // (แถม) พลังโจมตี (ใช้เฉพาะถ้าเป็น Weapon)
}