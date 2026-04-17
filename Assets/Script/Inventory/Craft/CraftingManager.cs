using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;

    [Header("UI References")]
    public GameObject craftingPanel; // ⭐ ลากตัว Panel หน้าต่างคราฟต์มาใส่ตรงนี้
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

    // ⭐ ฟังก์ชันใหม่สำหรับปิดหน้าต่าง (ใช้ผูกกับปุ่มปิดหน้าต่าง)
    public void CloseCraftingUI()
    {
        // 1. คืนของที่ค้างบนโต๊ะเข้ากระเป๋า
        CancelCrafting();

        // 2. ปิดหน้าต่าง UI
        if (craftingPanel != null) craftingPanel.SetActive(false);

        // 3. ปลดล็อกเม้าส์ (ถ้ามีระบบล็อก)
        Cursor.visible = true;

        // 4. ⭐ สำคัญที่สุด: สั่งให้ผู้เล่นเลิกท่าคราฟต์ทันที
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetCraftingState(false);
        }
    }

    public void CheckRecipe()
    {
        currentRecipe = null;

        if (resultPreviewImage != null)
        {
            resultPreviewImage.sprite = null;
            resultPreviewImage.enabled = false;
        }
        if (craftButton != null) craftButton.interactable = false;

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

            if (itemsOnTable.Count != requiredItems.Count) continue;

            if (CheckIngredientsExact(itemsOnTable, requiredItems))
            {
                currentRecipe = recipe;
                UpdateResultUI();
                return;
            }
        }
    }

    bool CheckIngredientsExact(List<ItemData> tableList, List<ItemData> recipeList)
    {
        List<ItemData> tempCheckList = new List<ItemData>(tableList);
        foreach (ItemData req in recipeList)
        {
            if (tempCheckList.Contains(req)) tempCheckList.Remove(req);
            else return false;
        }
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

    public void ConfirmCraft()
    {
        if (currentRecipe != null)
        {
            StartCoroutine(CraftingRoutine());
        }
    }

    private System.Collections.IEnumerator CraftingRoutine()
    {
        if (craftButton != null) craftButton.interactable = false;

        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetCraftingState(true);
        }

        yield return new WaitForSeconds(1.5f);

        Inventory.instance.AddItem(currentRecipe.result, currentRecipe.resultAmount);
        string craftedItemName = currentRecipe.result.itemName;

        foreach (var slot in inputSlots)
        {
            if (slot != null) slot.ClearSlot();
        }

        // ⭐ คืนสถานะหลังคราฟเสร็จ
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetCraftingState(false);
        }

        CheckRecipe();
        Debug.Log($"คราฟต์ {craftedItemName} สำเร็จ!");
    }

    public void CancelCrafting()
    {
        foreach (var slot in inputSlots)
        {
            if (slot != null && slot.itemInSlot != null && slot.amount > 0)
            {
                Inventory.instance.AddItem(slot.itemInSlot, slot.amount);
                slot.ClearSlot();
            }
        }

        // ⭐ เพิ่มบรรทัดนี้: เผื่อมีการกดยกเลิกกลางคัน ให้ตัวละครกลับมายืนปกติ
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetCraftingState(false);
        }

        CheckRecipe();
        Debug.Log("ยกเลิกและคืนของเรียบร้อย");
    }

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