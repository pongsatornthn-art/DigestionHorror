using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int amount;
    public InventoryItem(ItemData item, int qty) { itemData = item; amount = qty; }
    public void AddAmount(int value) => amount += value;
}

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space = 30;
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Equipment Settings")]
    public SpriteRenderer handRenderer;
    public ItemData currentEquippedItem;

    void Awake()
    {
        if (instance != null) return;
        instance = this;
        while (items.Count < space) items.Add(null);
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        InventoryItem existingItem = items.Find(i => i != null && i.itemData == item && i.itemData.isStackable && i.amount < i.itemData.maxStack);
        if (existingItem != null) { existingItem.AddAmount(amount); }
        else
        {
            int emptyIndex = items.FindIndex(i => i == null);
            if (emptyIndex != -1) items[emptyIndex] = new InventoryItem(item, amount);
            else return false;
        }
        onItemChangedCallback?.Invoke();
        return true;
    }

    // ✅ ฟังก์ชันเช็คไอเทมแบบระบุจำนวน (ป้องกัน Error ใน QuestObstacle)
    public bool HasItem(ItemData item, int amountRequired = 1)
    {
        int totalCount = 0;
        foreach (var slot in items)
        {
            if (slot != null && slot.itemData == item) totalCount += slot.amount;
        }
        return totalCount >= amountRequired;
    }

    // ✅ ฟังก์ชันลบไอเทมแบบระบุจำนวน (สำหรับระบบคราฟต์)
    public void RemoveItem(ItemData item, int amountToRemove = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemData == item)
            {
                if (items[i].amount > amountToRemove) { items[i].amount -= amountToRemove; amountToRemove = 0; }
                else { amountToRemove -= items[i].amount; if (currentEquippedItem == item) Unequip(); items[i] = null; }
            }
            if (amountToRemove <= 0) break;
        }
        onItemChangedCallback?.Invoke();
    }

    // ✅ แก้ Error CS1061 ใน ItemDrag.cs: เพิ่มฟังก์ชันสลับของ
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA >= 0 && indexA < space && indexB >= 0 && indexB < space)
        {
            InventoryItem temp = items[indexA];
            items[indexA] = items[indexB];
            items[indexB] = temp;
            onItemChangedCallback?.Invoke();
        }
    }

    public void EquipItem(ItemData itemToEquip)
    {
        currentEquippedItem = itemToEquip;
        if (handRenderer != null && itemToEquip != null) { handRenderer.sprite = itemToEquip.equippedSprite; handRenderer.enabled = true; }
    }

    public void Unequip()
    {
        currentEquippedItem = null;
        if (handRenderer != null) handRenderer.enabled = false;
    }
}