using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;

    // ⭐ เพิ่มบรรทัดนี้: เก็บเลขดัชนีที่แท้จริง
    [HideInInspector] public int slotIndex;

    ItemData item;
    bool canEquip;

    public void AddItem(ItemData newItem, int amount, bool isHotbar)
    {
        item = newItem;
        canEquip = isHotbar;
        icon.sprite = item.icon;

        // ทำให้มองเห็นเพื่อรองรับการลาก
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
        // ทำเป็นใสๆ เพื่อรอรับของ
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