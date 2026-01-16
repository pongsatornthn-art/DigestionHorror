using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<ItemData> items = new List<ItemData>();
    public int space = 20;

    // --- ส่วนที่เพิ่มมาใหม่ (ระบบถือของ) ---
    [Header("Equipment")]
    public SpriteRenderer handRenderer; // ลากตัว Hand มาใส่ตรงนี้
    public ItemData currentEquippedItem; // เก็บว่าตอนนี้ถืออะไรอยู่

    void Awake() { if (instance == null) instance = this; }

    public bool AddItem(ItemData item)
    {
        if (items.Count >= space) return false;
        items.Add(item);
        if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        return true;
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            // ถ้าของที่ลบ คือของที่ถืออยู่ -> ให้เอามือเปล่า
            if (currentEquippedItem == item) Unequip();

            items.Remove(item);
            if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        }
    }

    public bool HasItem(ItemData item) { return items.Contains(item); }

    // ฟังก์ชันถือของ (เรียกจากปุ่ม UI)
    public void EquipItem(ItemData itemToEquip)
    {
        currentEquippedItem = itemToEquip;

        // เปลี่ยนรูปที่มือ
        if (handRenderer != null)
        {
            handRenderer.sprite = itemToEquip.icon;
            handRenderer.enabled = true; // เปิดการมองเห็น
        }
        Debug.Log("ถืออาวุธ: " + itemToEquip.itemName);
    }

    // ฟังก์ชันถอดของ
    public void Unequip()
    {
        currentEquippedItem = null;
        if (handRenderer != null) handRenderer.enabled = false;
    }
}