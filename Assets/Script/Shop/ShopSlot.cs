using UnityEngine;
using UnityEngine.UI;
using TMPro; // ใช้สำหรับ TextMeshPro

public class ShopSlot : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public Button buyButton;

    private ItemData itemData;

    // ฟังก์ชันนี้ ShopUI จะเป็นคนเรียกเพื่อใส่ข้อมูลไอเทม
    public void SetupSlot(ItemData newItem)
    {
        itemData = newItem;

        if (itemData != null)
        {
            itemIcon.sprite = itemData.icon;
            itemNameText.text = itemData.itemName;
            itemPriceText.text = itemData.price.ToString() + " $"; // ดึงราคาจาก ItemData มาโชว์

            itemIcon.enabled = true;
            buyButton.interactable = true;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        itemData = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemNameText.text = "";
        itemPriceText.text = "";
        buyButton.interactable = false;
    }
}