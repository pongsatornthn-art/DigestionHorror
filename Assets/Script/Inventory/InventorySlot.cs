using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;

    // เก็บเลขดัชนีที่แท้จริง
    [HideInInspector] public int slotIndex;

    // เปลี่ยนเป็น public เพื่อให้ ItemDrag มองเห็นได้ง่ายๆ
    public ItemData item;

    bool canEquip;

    void Start()
    {
        // ⭐ เพิ่มส่วนนี้: บังคับเปิดการมองเห็นของเมาส์ผ่านโค้ดเลย (กันลืมติ๊กใน Inspector)
        if (icon != null)
        {
            icon.raycastTarget = true;
        }
    }

    public void AddItem(ItemData newItem, int amount, bool isHotbar)
    {
        item = newItem;
        canEquip = isHotbar;

        icon.sprite = item.icon;

        // ทำให้มองเห็นรูปไอเท็มชัดเจน
        icon.color = Color.white;
        icon.enabled = true;

        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
            amountText.enabled = amount > 1;
        }
    }

    public void ClearSlot()
    {
        item = null;

        // ⭐ จุดสำคัญ: ทำให้เป็นสีใส แต่ยังเปิด enabled ไว้ เพื่อให้เมาส์วางของใส่ช่องว่างได้
        icon.sprite = null;
        icon.color = Color.clear;
        icon.enabled = true;

        if (amountText != null) amountText.enabled = false;
        canEquip = false;
    }

    public void OnUseButton()
    {
        if (item != null && canEquip)
        {
            Inventory.instance.EquipItem(item);
        }
    }
}