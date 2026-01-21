using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon;              // รูปไอคอนเล็กๆ ในช่องเก็บของ

    // ⭐ เพิ่มบรรทัดนี้: เอารูปใหญ่ๆ ที่เป็นคำอธิบายมาใส่ช่องนี้
    public Sprite descriptionImage;

    public bool isStackable = true;
    public int maxStack = 99;
    public Sprite equippedSprite;
}