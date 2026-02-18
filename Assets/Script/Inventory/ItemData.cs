using UnityEngine;

public enum ItemType { General, Weapon, Totem, Consumable }

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
    public float lightAttackCooldown = 0.5f; // ⭐ เพิ่มใหม่: คูลดาวน์โจมตีเบา

    [Header("Heavy Attack")]
    public int heavyDamage = 20;
    public float heavyStaminaCost = 25f;
    public float heavyKnockback = 6f;
    public float heavyAttackCooldown = 1.2f; // ⭐ เพิ่มใหม่: คูลดาวน์โจมตีหนัก
}