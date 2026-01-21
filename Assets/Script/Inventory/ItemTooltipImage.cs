using UnityEngine;
using UnityEngine.UI; // ⭐ เราต้องใช้ UnityEngine.UI เพราะจะคุม Image

public class ItemTooltipImage : MonoBehaviour
{
    // Singleton instance เพื่อให้เรียกใช้ง่ายๆ
    public static ItemTooltipImage instance;

    // ลาก UI Image จากในกรอบสีแดงมาใส่ช่องนี้
    public Image targetImageDisplay;

    private void Awake()
    {
        instance = this;
        // เริ่มเกมมาสั่งซ่อนรูปไปก่อน
        ClearDescriptionImage();
    }

    // ฟังก์ชันโชว์รูป
    public void ShowDescriptionImage(Sprite newSprite)
    {
        // ถ้ารูปที่ส่งมามีจริง ค่อยแสดง
        if (newSprite != null)
        {
            targetImageDisplay.sprite = newSprite;
            targetImageDisplay.enabled = true; // เปิดการมองเห็น
        }
        else
        {
            // ถ้าไอเทมไม่มีรูปคำอธิบาย ก็ให้ซ่อนไป
            ClearDescriptionImage();
        }
    }

    // ฟังก์ชันซ่อนรูป (ตอนเมาส์ออก)
    public void ClearDescriptionImage()
    {
        targetImageDisplay.sprite = null;
        targetImageDisplay.enabled = false; // ปิดการมองเห็น ทำให้รูปหายไป
    }
}