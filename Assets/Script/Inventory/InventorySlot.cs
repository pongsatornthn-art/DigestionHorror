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
        if (item == null) return;

        if (item.descriptionImage != null && ItemTooltipImage.instance != null)
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
    // ⭐ จัดการไอเทม
    // ==========================================

    public void AddItem(ItemData newItem, int amount, bool isHotbar)
    {
        item = newItem;
        canEquip = isHotbar;

        if (item != null && item.icon != null)
        {
            icon.sprite = item.icon;
            icon.color = Color.white;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.color = Color.clear;
            icon.enabled = true;
        }

        if (amountText != null)
        {
            amountText.text = amount.ToString();
            amountText.enabled = true;
        }
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.color = Color.clear;
        icon.enabled = true;

        if (amountText != null)
        {
            amountText.text = "";
            amountText.enabled = false;
        }

        canEquip = false;

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

    // ==========================================
    // ⭐ ส่วนที่เพิ่มใหม่: จัดการเวลากดใช้ไอเทม
    // ==========================================
    // ในไฟล์ InventorySlot.cs
    public void OnUseItem()
    {
        // [Debug 1] เช็คว่าปุ่มถูกกดจริงไหม
        Debug.Log("👆 กดปุ่มที่ช่องกระเป๋าแล้ว! (OnUseItem ทำงาน)");

        if (item == null)
        {
            Debug.Log("❌ แต่ในช่องนี้ไม่มีไอเทม (item เป็น null)");
            return;
        }

        // [Debug 2] เช็คว่ามันมองเห็นไอเทมถูกต้องไหม
        Debug.Log("📦 ไอเทมในช่องคือ: " + item.itemName + " | ประเภท: " + item.itemType);

        if (item.itemType == ItemType.Consumable)
        {
            // [Debug 3] เช็คว่ามันเจอกับระบบ DigestionSystem ไหม
            if (DigestionSystem.instance != null)
            {
                Debug.Log("✅ เจอ DigestionSystem! กำลังสั่งให้ลดค่าลง: " + item.digestionReduceAmount);

                // เรียกฟังก์ชันลดค่า
                DigestionSystem.instance.DecreaseDigestion(item.digestionReduceAmount);
            }
            else
            {
                Debug.LogError("😱 หา 'DigestionSystem.instance' ไม่เจอ! (คุณลืมวาง DigestionSystem ในฉาก หรือลืมเขียน Awake() หรือเปล่า?)");
            }

            // ลบไอเทม 1 ชิ้น
            Inventory.instance.RemoveItem(item);
        }
        // ... (ส่วนของ Totem) ...
    }

    internal void UpdateUI()
    {
        throw new NotImplementedException();
    }
}