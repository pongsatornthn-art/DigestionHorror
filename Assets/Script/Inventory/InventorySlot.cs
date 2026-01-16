using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    ItemData item; // เก็บข้อมูลไอเท็มในช่องนี้

    public void AddItem(ItemData newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    // --- ส่วนที่เพิ่มมาใหม่ (กดปุ่มเพื่อใช้) ---
    // ฟังก์ชันนี้จะถูกเรียกเมื่อเรากดปุ่ม UI
    public void OnUseButton()
    {
        if (item != null)
        {
            Inventory.instance.EquipItem(item);
        }
    }
}