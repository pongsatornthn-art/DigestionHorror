using UnityEngine;
using System.Collections.Generic;
using System;

// 1. สร้างคลาสสำหรับเก็บข้อมูลของ + จำนวน (Stack)
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
    // 2. ระบบ Singleton (เพื่อให้ไฟล์อื่นเรียกใช้ได้ง่ายๆ)
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("มี Inventory มากกว่า 1 อัน! กำลังลบอันเกิน...");
            return;
        }
        instance = this;
    }

    // 3. ตัวแปรเก็บของ
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<InventoryItem> items = new List<InventoryItem>(); // รายการของในกระเป๋า
    public int space = 9; // จำนวนช่องเก็บของ (Hotbar)

    [Header("Equipment Settings")]
    public SpriteRenderer handRenderer;   // ลากตัว Hand มาใส่ตรงนี้
    public ItemData currentEquippedItem;  // เก็บว่าถืออะไรอยู่

    // 4. ฟังก์ชันเพิ่มไอเท็ม (AddItem)
    public bool AddItem(ItemData item)
    {
        // เช็คว่ามีของชิ้นนี้อยู่แล้วไหม?
        InventoryItem existingItem = items.Find(i => i.itemData == item);

        if (existingItem != null)
        {
            // ถ้ามีแล้ว -> ให้เพิ่มจำนวน (Stack)
            existingItem.AddAmount(1);
        }
        else
        {
            // ถ้ายังไม่มี -> เช็คที่ว่าง
            if (items.Count >= space)
            {
                Debug.Log("กระเป๋าเต็ม!");
                return false;
            }
            // เพิ่มช่องใหม่
            items.Add(new InventoryItem(item, 1));
        }

        // แจ้งเตือนหน้าจอให้อัปเดต
        if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        return true;
    }

    // 5. ฟังก์ชันลบไอเท็ม (RemoveItem)
    public void RemoveItem(ItemData item)
    {
        InventoryItem existingItem = items.Find(i => i.itemData == item);

        if (existingItem != null)
        {
            existingItem.amount--; // ลดจำนวนลง 1

            // ถ้าเหลือ 0 ให้ลบทิ้ง
            if (existingItem.amount <= 0)
            {
                if (currentEquippedItem == item) Unequip(); // ถอดจากมือก่อน
                items.Remove(existingItem);
            }

            if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        }
    }

    // 6. ฟังก์ชันเช็คของ (HasItem) - ใช้กับระบบ Craft
    public bool HasItem(ItemData itemToCheck)
    {
        InventoryItem found = items.Find(i => i.itemData == itemToCheck);
        return found != null && found.amount > 0;
    }

    // 7. ฟังก์ชันถือของ (Equip)
    public void EquipItem(ItemData itemToEquip)
    {
        currentEquippedItem = itemToEquip;
        if (handRenderer != null)
        {
            handRenderer.sprite = itemToEquip.icon;
            handRenderer.enabled = true;
        }
    }

    // 8. ฟังก์ชันถอดของ (Unequip)
    public void Unequip()
    {
        currentEquippedItem = null;
        if (handRenderer != null) handRenderer.enabled = false;
    }

    internal void SwapItems(int slotIndex1, int slotIndex2)
    {
        throw new NotImplementedException();
    }
}