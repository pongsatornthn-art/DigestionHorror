using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon;              // รูปในกระเป๋า (UI)

    // ⭐ ส่วนที่ Error บอกว่าขาดไป (ต้องเพิ่มเข้ามาครับ)
    public bool isStackable = true;  // เก็บซ้อนกันได้ไหม?
    public int maxStack = 99;        // ซ้อนได้สูงสุดเท่าไหร่?
    public Sprite equippedSprite;    // รูปตอนถือในฉากเกม
}