using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;

    [Header("UI References")]
    public CraftingSlot[] inputSlots;
    public Image resultPreviewImage;
    public Button craftButton; 

    [Header("Data")]
    public List<CraftingRecipe> recipes;
    private CraftingRecipe currentRecipe;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (resultPreviewImage != null) resultPreviewImage.enabled = false;
        if (craftButton != null) craftButton.interactable = false;
    }

    public void CheckRecipe()
    {
        currentRecipe = null;

        // รีเซ็ต UI ก่อนเริ่มเช็ค
        if (resultPreviewImage != null)
        {
            resultPreviewImage.sprite = null;
            resultPreviewImage.enabled = false;
        }
        if (craftButton != null) craftButton.interactable = false;

        // 1. รวบรวมไอเทมทั้งหมดบนโต๊ะ "แตกเป็นชิ้นๆ" ใส่ List
        // (เช่น ไม้ x2 จะถูกแตกเป็น [ไม้, ไม้] เพื่อให้เช็คง่าย)
        List<ItemData> itemsOnTable = new List<ItemData>();
        foreach (var slot in inputSlots)
        {
            if (slot.itemInSlot != null && slot.amount > 0)
            {
                for (int i = 0; i < slot.amount; i++)
                {
                    itemsOnTable.Add(slot.itemInSlot);
                }
            }
        }

        foreach (CraftingRecipe recipe in recipes)
        {
            if (recipe == null) continue;

            List<ItemData> requiredItems = new List<ItemData>();
            foreach (var ing in recipe.ingredients)
            {
                for (int i = 0; i < ing.amount; i++) requiredItems.Add(ing.item);
            }

            // ⭐ กฎเหล็กข้อที่ 1: "จำนวนต้องเท่ากันเป๊ะ"
            // ถ้าบนโต๊ะมี 3 ชิ้น แต่สูตรใช้ 2 ชิ้น -> ปัดตกทันที (แก้บั๊ก A+B ได้ Z)
            if (itemsOnTable.Count != requiredItems.Count)
            {
                continue;
            }

            // ⭐ กฎเหล็กข้อที่ 2: "ไส้ในต้องเหมือนกัน"
            if (CheckIngredientsExact(itemsOnTable, requiredItems))
            {
                // เจอสูตรที่ถูกต้อง!
                currentRecipe = recipe;
                UpdateResultUI();
                return; // หยุดหาทันที
            }
        }
    }

    // ฟังก์ชันช่วยเช็คว่าของตรงกันไหม (ไม่สนลำดับการวาง)
    bool CheckIngredientsExact(List<ItemData> tableList, List<ItemData> recipeList)
    {
        // สร้างรายการจำลองมาเพื่อขีดฆ่าออก
        List<ItemData> tempCheckList = new List<ItemData>(tableList);

        foreach (ItemData req in recipeList)
        {
            if (tempCheckList.Contains(req))
            {
                tempCheckList.Remove(req); // เจอแล้วลบออก 1 ชิ้น
            }
            else
            {
                return false; // หาไม่เจอแสดงว่าสูตรผิด
            }
        }

        // เช็คครั้งสุดท้าย: ต้องไม่มีของเหลือในรายการจำลอง
        return tempCheckList.Count == 0;
    }

    void UpdateResultUI()
    {
        if (currentRecipe != null)
        {
            if (resultPreviewImage != null)
            {
                resultPreviewImage.sprite = currentRecipe.result.icon;
                resultPreviewImage.enabled = true;
            }
            if (craftButton != null) craftButton.interactable = true;
        }
    }

    // ---------------------------------------------------------
    // 🛠️ ส่วน Action (คราฟต์, ยกเลิก, คืนของ)
    // ---------------------------------------------------------

    public void ConfirmCraft()
    {
        if (currentRecipe != null)
        {
            Inventory.instance.AddItem(currentRecipe.result, currentRecipe.resultAmount);

            // 2. ล้างของบนโต๊ะทิ้ง (เพราะใช้ไปแล้ว)
            foreach (var slot in inputSlots)
            {
                if (slot != null) slot.ClearSlot();
            }

            // 3. รีเซ็ตระบบ
            CheckRecipe();
            Debug.Log($"คราฟต์ {currentRecipe.result.itemName} สำเร็จ!");
        }
    }

    public void CancelCrafting()
    {
        foreach (var slot in inputSlots)
        {
            if (slot != null && slot.itemInSlot != null && slot.amount > 0)
            {
                // คืนของเข้ากระเป๋า
                Inventory.instance.AddItem(slot.itemInSlot, slot.amount);
                slot.ClearSlot();
            }
        }
        CheckRecipe();
        Debug.Log("ยกเลิกและคืนของเรียบร้อย");
    }

    // Helper Functions
    public int GetTotalItemInInventory(ItemData item)
    {
        if (Inventory.instance != null) return Inventory.instance.GetItemCount(item);
        return 0;
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
}