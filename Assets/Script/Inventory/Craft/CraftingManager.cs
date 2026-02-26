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

    // ==========================================
    // ⭐ ส่วนที่แก้ไข: เพิ่มระบบหน่วงเวลาคราฟและเล่นอนิเมชั่น
    // ==========================================
    public void ConfirmCraft()
    {
        if (currentRecipe != null)
        {
            // เปลี่ยนไปใช้ Coroutine เพื่อให้รอเวลาได้
            StartCoroutine(CraftingRoutine());
        }
    }

    private System.Collections.IEnumerator CraftingRoutine()
    {
        // 1. ปิดปุ่มคราฟชั่วคราว ป้องกันผู้เล่นกดเบิ้ลรัวๆ
        if (craftButton != null) craftButton.interactable = false;

        // 2. สั่งให้ Player เล่นอนิเมชั่นก้มคราฟของ
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetCraftingState(true);
        }

        // 3. หน่วงเวลาคราฟ (ปรับตัวเลข 1.5f ได้ตามต้องการว่าอยากให้ก้มนานแค่ไหน)
        yield return new WaitForSeconds(1.5f);

        // 4. ให้ของรางวัลเข้ากระเป๋า
        Inventory.instance.AddItem(currentRecipe.result, currentRecipe.resultAmount);
        string craftedItemName = currentRecipe.result.itemName;

        // 5. ล้างของบนโต๊ะทิ้ง
        foreach (var slot in inputSlots)
        {
            if (slot != null) slot.ClearSlot();
        }

        // 6. สั่งให้ Player เลิกเล่นอนิเมชั่นก้ม และกลับมายืนปกติ
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetCraftingState(false);
        }

        // 7. รีเซ็ต UI โต๊ะคราฟ
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