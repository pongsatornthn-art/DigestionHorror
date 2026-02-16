using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlot : MonoBehaviour, IDropHandler
{
    public ItemData itemInSlot;
    public int amount = 0;
    public Image iconImage;
    public TextMeshProUGUI amountText;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        ItemDrag itemDrag = eventData.pointerDrag.GetComponent<ItemDrag>();

        if (itemDrag != null && itemDrag.mySlot != null)
        {
            ItemData data = itemDrag.mySlot.item;
            if (data == null) return;

            if (CraftingManager.instance == null) return;

            // 1. เช็คจำนวนรวมในกระเป๋า เทียบกับบนโต๊ะ
            int totalOwned = Inventory.instance.GetItemCount(data);
            int alreadyOnTable = CraftingManager.instance.GetAmountOnTable(data);

            if (alreadyOnTable < totalOwned)
            {
                if (itemInSlot == null || itemInSlot == data)
                {
                    // 2. เพิ่มของลงช่องคราฟต์
                    AddItemToCraft(data, 1);

                    // 3. ✨ จุดสำคัญ: หักของออกจาก Inventory ระบบหลักจริงๆ
                    // วิธีนี้จะทำให้รูปที่ Hotbar หายไปอัตโนมัติหากคุณเขียนระบบ UI ลิงก์กับ List ไว้
                    Inventory.instance.RemoveItem(data, 1);

                    Debug.Log($"วาง {data.itemName} และหักจาก Inventory สำเร็จ!");
                }
            }
            else
            {
                Debug.LogWarning("ของในตัวหมดแล้ว วางเพิ่มไม่ได้!");
            }
        }
    } // จบ OnDrop

    public void AddItemToCraft(ItemData item, int qty)
    {
        itemInSlot = item;
        amount += qty;
        UpdateUI();
        if (CraftingManager.instance != null) CraftingManager.instance.CheckRecipe();
    }

    public void UpdateUI()
    {
        if (iconImage == null) return;

        if (itemInSlot != null)
        {
            iconImage.sprite = itemInSlot.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white;
            if (amountText != null)
                amountText.text = amount > 1 ? amount.ToString() : "";
        }
        else
        {
            iconImage.enabled = false;
            if (amountText != null) amountText.text = "";
        }
    }

    public void ClearSlot()
    {
        itemInSlot = null;
        amount = 0;
        UpdateUI();
    }
} // จบ Class