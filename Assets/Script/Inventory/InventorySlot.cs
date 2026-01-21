using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ต้องมีบรรทัดนี้

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    [HideInInspector] public int slotIndex;

    public ItemData item;
    bool canEquip;

    // ❌ ลบ Start() ที่ไปปิด Raycast ออก เพื่อให้ลากของได้ปกติครับ

    // ==========================================
    // ⭐ ส่วนตรวจสอบเมาส์ (Mouse Hover)
    // ==========================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. ค้นหาว่าหน้า InventoryPanel เปิดอยู่ไหม?
        GameObject inventoryPanel = GameObject.Find("InventoryPanel");

        // ถ้าหาไม่เจอ หรือ มันปิดอยู่ -> ไม่ต้องโชว์รูป
        if (inventoryPanel != null && !inventoryPanel.activeInHierarchy)
        {
            return;
        }

        // 2. ถ้ากระเป๋าเปิดอยู่ ค่อยโชว์รูป
        if (item != null && item.descriptionImage != null && ItemTooltipImage.instance != null)
        {
            ItemTooltipImage.instance.ShowDescriptionImage(item.descriptionImage);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltipImage.instance != null)
        {
            ItemTooltipImage.instance.ClearDescriptionImage();
        }
    }

    // ==========================================
    // ส่วนฟังก์ชันจัดการไอเทม
    // ==========================================

    public void AddItem(ItemData newItem, int amount, bool isHotbar)
    {
        item = newItem;
        canEquip = isHotbar;

        icon.sprite = item.icon;
        icon.color = Color.white;
        icon.enabled = true; // เปิดให้มองเห็นและลากได้

        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
            amountText.enabled = amount > 1;
        }
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.color = Color.clear;
        icon.enabled = true; // เปิดไว้เพื่อรับของ (Drop Area)

        if (amountText != null) amountText.enabled = false;
        canEquip = false;

        // กันเหนียว: ถ้าของหายไปขณะเมาส์ชี้อยู่ ให้เอารูปออกด้วย
        if (ItemTooltipImage.instance != null)
        {
            ItemTooltipImage.instance.ClearDescriptionImage();
        }
    }

    public void OnUseButton()
    {
        if (item != null && canEquip)
        {
            Inventory.instance.EquipItem(item);
        }
    }
}