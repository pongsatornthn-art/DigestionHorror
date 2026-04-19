using UnityEngine;
using UnityEngine.Scripting;

public enum ItemType { General, Weapon, Totem, Consumable }

[Preserve]
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("General Info")]
    public string itemName = "New Item";
    public Sprite icon;
    [TextArea] public string description;
    public Sprite descriptionImage;

    [Header("Stacking & Type")]
    public bool isStackable = true;
    public int maxStack = 99;
    public ItemType itemType;

    [Header("Equipment Visuals")]
    public Sprite equippedSprite;

    [Header("Animation Settings")]
    public AnimatorOverrideController weaponAnimatorOverride;

    [Header("Combat Stats (Only for Weapons)")]
    public int weaponID;

    [Header("Light Attack")]
    public int damage = 10;
    public float staminaCost = 10f;
    public float knockback = 3f;
    public float lightAttackCooldown = 0.5f;

    [Header("Heavy Attack")]
    public int heavyDamage = 20;
    public float heavyStaminaCost = 25f;
    public float heavyKnockback = 6f;
    public float heavyAttackCooldown = 1.2f;

    [Header("Durability")]
    public float maxDurability;

    [Header("Economy (ระบบเงิน)")]
    public int price = 50;

    [Header("Consumable Stats (สำหรับ ยา/อาหาร)")]
    public float digestionReduceAmount = 20f;
    public int healAmount = 50; // ⭐ เพิ่มช่องนี้: เอาไว้ตั้งค่าว่าไอเทมนี้ฮีลเลือดเท่าไหร่

    [Header("Totem Stats (สำหรับ โทเทม)")]
    public float digestionSlowMultiplier = 0.5f;
    public float totemEffectDuration = 60f;

    // ==========================================
    // ⭐ ส่วนที่เพิ่มใหม่: ระบบสถานะพิเศษ (ตีเลือดไหล)
    // ==========================================
    [Header("Special Effects (สถานะพิเศษ)")]
    public bool causesBleeding = false; // ติ๊กถูกถ้าเป็นไม้ตะปู
    public float bleedDuration = 10f;   // ระยะเวลาเลือดไหล (10 วินาที)
    public int bleedDamagePerSec = 2;   // ดาเมจที่ลดลงทุกๆ 1 วินาที

}