using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;
    public CraftingSlot[] inputSlots;
    public Image resultPreviewImage;
    public Button craftButton;
    public List<CraftingRecipe> recipes;
    private CraftingRecipe currentRecipe;

    void Awake()
    {
        instance = this;
    }

    public void CheckRecipe()
    {
        Dictionary<ItemData, int> currentInTable = new Dictionary<ItemData, int>();
        foreach (var slot in inputSlots)
        {
            if (slot.itemInSlot != null)
            {
                if (currentInTable.ContainsKey(slot.itemInSlot))
                    currentInTable[slot.itemInSlot] += slot.amount;
                else
                    currentInTable.Add(slot.itemInSlot, slot.amount);
            }
        }

        currentRecipe = null;
        if (resultPreviewImage != null) resultPreviewImage.enabled = false;
        if (craftButton != null) craftButton.interactable = false;

        foreach (var recipe in recipes)
        {
            // ✅ ข้ามสูตรที่ว่าง (None) เพื่อไม่ให้ Error
            if (recipe == null) continue;

            if (IsMatch(recipe, currentInTable))
            {
                currentRecipe = recipe;
                if (resultPreviewImage != null)
                {
                    resultPreviewImage.sprite = recipe.result.icon;
                    resultPreviewImage.enabled = true;
                }
                if (craftButton != null) craftButton.interactable = true;
                return;
            }
        }

    }


    bool IsMatch(CraftingRecipe recipe, Dictionary<ItemData, int> currentInTable)
    {
        foreach (var ing in recipe.ingredients)
        {
            if (!currentInTable.ContainsKey(ing.item) || currentInTable[ing.item] < ing.amount)
                return false;
        }
        return true;
    }
    // เพิ่มฟังก์ชันนี้ใน CraftingManager.cs ครับ
    public int GetTotalItemInInventory(ItemData item)
    {
        // เรียกไปที่ระบบ Inventory ของคุณพงศธรเพื่อนับจำนวนไอเทมชิ้นนี้ทั้งหมดที่มี
        if (Inventory.instance != null)
        {
            return Inventory.instance.GetItemCount(item);
        }
        return 0;
    }

    public void ConfirmCraft()
    {
        if (currentRecipe != null)
        {

            Inventory.instance.AddItem(currentRecipe.result);

            foreach (var slot in inputSlots)
            {
                if (slot != null) slot.ClearSlot();
            }

            CheckRecipe();
            Debug.Log("คราฟต์เสร็จสมบูรณ์!");
        }
    }

    public int GetAmountOnTable(ItemData item)
    {
        int total = 0;
        foreach (var slot in inputSlots)
        {
            if (slot.itemInSlot == item) total += slot.amount;
        }
        return total;
    }
    public void CancelCrafting()
    {
        // วนลูปเช็คทุกช่องในโต๊ะคราฟต์
        foreach (var slot in inputSlots)
        {
            // ถ้าในช่องมีไอเทมค้างอยู่
            if (slot != null && slot.itemInSlot != null && slot.amount > 0)
            {
                // 1. เพิ่มไอเทมคืนกลับเข้าไปใน Inventory
                Inventory.instance.AddItem(slot.itemInSlot, slot.amount);

                // 2. ล้างข้อมูลในช่องคราฟต์นั้นทิ้ง
                slot.ClearSlot();
            }
        }

        // 3. ตรวจสอบสูตรใหม่ (ซึ่งควรจะกลายเป็นว่างเปล่า)
        CheckRecipe();

        Debug.Log("ยกเลิกการคราฟต์และคืนไอเทมทั้งหมดเข้ากระเป๋าแล้ว");
    }

}