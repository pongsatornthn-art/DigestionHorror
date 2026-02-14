using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int amount;

    public InventoryItem(ItemData item, int qty)
    {
        itemData = item;
        amount = qty;
    }

    public void AddAmount(int value) => amount += value;
}

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space = 30; // จำนวนช่องทั้งหมด

    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Equipment Settings")]
    public SpriteRenderer handRenderer;
    public ItemData currentEquippedItem;

    void Awake()
    {
        if (instance != null) return;
        instance = this;

        while (items.Count < space)
        {
            items.Add(null);
        }
    }

    // ✅ ฟังก์ชัน AddItem ที่ถูกต้อง (จัดการ Data)
    public bool AddItem(ItemData item, int amount = 1)
    {
        // 1. ลองหาของเดิมเพื่อ Stack (ต้องไม่เป็น null, ชื่อตรงกัน, stackable, และยังไม่เต็ม)
        InventoryItem existingItem = items.Find(i => i != null && i.itemData == item && i.itemData.isStackable && i.amount < i.itemData.maxStack);

        if (existingItem != null)
        {
            // ถ้าเจอของซ้ำ -> บวกจำนวนเพิ่ม
            existingItem.AddAmount(amount);
        }
        else
        {
            // 2. ถ้าไม่มี -> หา "ช่องว่างแรก" (ที่เป็น null)
            int emptyIndex = items.FindIndex(i => i == null);

            if (emptyIndex != -1) // ถ้าเจอช่องว่าง
            {
                items[emptyIndex] = new InventoryItem(item, amount);
            }
            else
            {
                Debug.Log("Inventory Full!");
                return false;
            }
        }

        // แจ้งเตือน UI ให้วาดรูปใหม่
        if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        return true;
    }

    // Overload เผื่อเรียกใช้ง่ายๆ (เช่นเก็บของชิ้นเดียว)
    public bool AddItem(ItemData item) => AddItem(item, 1);

    public void RemoveItem(ItemData item)
    {
        // หาช่องที่มีของนี้อยู่
        int itemIndex = items.FindIndex(i => i != null && i.itemData == item);

        if (itemIndex != -1)
        {
            items[itemIndex].amount--;

            if (items[itemIndex].amount <= 0)
            {
                if (currentEquippedItem == item) Unequip();

                // เปลี่ยนช่องนั้นกลับเป็น null (ว่าง)
                items[itemIndex] = null;
            }

            if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        }
    }

    public bool HasItem(ItemData itemToCheck)
    {
        return items.Exists(i => i != null && i.itemData == itemToCheck);
    }

    public void EquipItem(ItemData itemToEquip)
    {
        currentEquippedItem = itemToEquip;
        if (handRenderer != null && itemToEquip != null)
        {
            handRenderer.sprite = itemToEquip.equippedSprite;
            handRenderer.enabled = true;
        }
    }

    public void Unequip()
    {
        currentEquippedItem = null;
        if (handRenderer != null) handRenderer.enabled = false;
    }

    // ฟังก์ชันสลับของ (Logic)
    public void SwapItems(int indexA, int indexB)
    {
        // เช็คแค่ว่า Index อยู่ในขอบเขต 0-29 ไหม
        if (indexA >= 0 && indexA < space && indexB >= 0 && indexB < space)
        {
            InventoryItem temp = items[indexA];
            items[indexA] = items[indexB];
            items[indexB] = temp;

            if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        }
    }
}