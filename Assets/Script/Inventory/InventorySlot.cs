using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    [HideInInspector] public int slotIndex;

    public ItemData item;
    bool canEquip;
    internal ItemData itemInSlot;
    internal int amount;

    // ==========================================
    // ⭐ ส่วนตรวจสอบเมาส์ (Mouse Hover)
    // ==========================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ถ้าไม่มีไอเทม ไม่ต้องโชว์อะไร
        if (item == null) return;

        // เช็คว่าระบบ Tooltip พร้อมใช้งานไหม
        if (item.descriptionImage != null && ItemTooltipImage.instance != null)
        {
            // สั่งโชว์รูปคำอธิบาย
            ItemTooltipImage.instance.ShowDescriptionImage(item.descriptionImage);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // เมื่อเมาส์ออก ให้สั่งปิด Tooltip
        if (ItemTooltipImage.instance != null)
        {
            ItemTooltipImage.instance.ClearDescriptionImage();
        }
    }

    // ==========================================
    // ⭐ แก้ไขส่วนนี้: จัดการไอเทม (แก้บั๊กรูปขาว + ตัวเลข)
    // ==========================================

    public void AddItem(ItemData newItem, int amount, bool isHotbar)
    {
        item = newItem;
        canEquip = isHotbar;

        // --- 1. จัดการรูปภาพ (Icon) ---
        // เช็คกันเหนียว: ถ้าไอเทมมีอยู่จริง และมีรูป
        if (item != null && item.icon != null)
        {
            icon.sprite = item.icon;
            icon.color = Color.white; // ปรับสีให้ชัด
            icon.enabled = true;      // เปิดการแสดงผล
        }
        else
        {
            // ถ้าไม่มีรูป (เช่น ลืมใส่ใน Inspector) ให้ทำเป็นใสๆ ไว้ กันเป็นสี่เหลี่ยมขาว
            icon.sprite = null;
            icon.color = Color.clear;
            icon.enabled = true;
        }

        // --- 2. จัดการตัวเลข (Amount Text) ---
        if (amountText != null)
        {
            // ให้โชว์เลขเสมอ (แม้จะมี 1 ชิ้น) จะได้รู้ว่าโค้ดทำงานถูกไหม
            amountText.text = amount.ToString();
            amountText.enabled = true;
        }
    }

    public void ClearSlot()
    {
        item = null;

        // เคลียร์รูป: ให้เป็นสีใส (Transparent) แต่ยังเปิด enabled ไว้เพื่อให้รับการลากของใส่ได้ (Drop)
        icon.sprite = null;
        icon.color = Color.clear;
        icon.enabled = true;

        if (amountText != null)
        {
            amountText.text = "";
            amountText.enabled = false;
        }

        canEquip = false;

        // ถ้าของหายไปขณะเมาส์ชี้อยู่ (เช่น กินยาหมด) ให้ปิด Tooltip ด้วย
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

    internal void UpdateUI()
    {
        throw new NotImplementedException();
    }
}