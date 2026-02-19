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
    // ⭐ ประกาศทั้ง 2 แบบเพื่อแก้ Error ในสคริปต์อื่นๆ ทั้งโปรเจกต์
    public static Inventory Instance;
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
        // ⭐ เซ็ตค่าให้ทั้งคู่ชี้มาที่สคริปต์นี้
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        instance = this;

        // เติมช่องว่างให้ครบตามจำนวน space
        while (items.Count < space) items.Add(null);
    }

    // --- ระบบจัดการไอเทม ---

    public bool AddItem(ItemData item, int amount = 1)
    {
        // 1. เช็คว่ามีของกองเดิมที่ยังไม่เต็มไหม (สำหรับของที่ Stack ได้)
        InventoryItem existingItem = items.Find(i => i != null && i.itemData == item && i.itemData.isStackable && i.amount < i.itemData.maxStack);

        if (existingItem != null)
        {
            existingItem.AddAmount(amount);
        }
        else
        {
            // 2. ถ้าไม่มีกองเดิม ให้หาช่องว่างช่องแรก
            int emptyIndex = items.FindIndex(i => i == null);
            if (emptyIndex != -1)
            {
                items[emptyIndex] = new InventoryItem(item, amount);
            }
            else
            {
                Debug.Log("กระเป๋าเต็ม!");
                return false;
            }
        }

        onItemChangedCallback?.Invoke();
        return true;
    }

    // ⭐ ฟังก์ชันสำหรับระบบซ่อมอาวุธ
    public int CheckItem(ItemData item)
    {
        return GetItemCount(item);
    }

    public int GetItemCount(ItemData item)
    {
        if (items == null) return 0;
        int total = 0;
        foreach (var slot in items)
        {
            if (slot != null && slot.itemData == item) total += slot.amount;
        }
        return total;
    }

    public bool HasItem(ItemData item, int amountRequired = 1)
    {
        return GetItemCount(item) >= amountRequired;
    }

    public void RemoveItem(ItemData item, int amountToRemove = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemData == item)
            {
                if (items[i].amount > amountToRemove)
                {
                    items[i].amount -= amountToRemove;
                    amountToRemove = 0;
                }
                else
                {
                    amountToRemove -= items[i].amount;
                    if (currentEquippedItem == item) Unequip();
                    items[i] = null;
                }
            }
            if (amountToRemove <= 0) break;
        }
        onItemChangedCallback?.Invoke();
    }

    // --- ระบบ UI และการสวมใส่ ---

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
        if (handRenderer != null && itemToEquip != null)
        {
            handRenderer.sprite = itemToEquip.equippedSprite;
            handRenderer.enabled = true;
        }

        // ส่งข้อมูลไปบอก PlayerController ให้สลับโมเดลในมือด้วย
        if (PlayerController.instance != null)
        {
            PlayerController.instance.EquipWeapon(itemToEquip);
        }
    }

    public void Unequip()
    {
        currentEquippedItem = null;
        if (handRenderer != null) handRenderer.enabled = false;

        // บอก PlayerController ให้เก็บอาวุธ (มือเปล่า)
        if (PlayerController.instance != null)
        {
            PlayerController.instance.EquipWeapon(null);
        }
    }

    public InventoryItem GetItemAt(int index)
    {
        if (index >= 0 && index < items.Count) return items[index];
        return null;
    }
}