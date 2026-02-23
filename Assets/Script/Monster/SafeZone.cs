using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Settings")]
    public float reduceAmount = 5f; // ค่า Digestion จะลดลงวินาทีละเท่าไหร่ตอนอยู่ในบ้าน

    // ใช้ OnTriggerEnter เพื่อเปิดโหมดปลอดภัยทันทีที่เหยียบเข้าบ้าน
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🛡️ เข้า Safe Zone แล้ว! ปลอดภัยจาก Watching Hour");

            if (WatchingHourManager.instance != null)
            {
                WatchingHourManager.instance.isPlayerSafe = true;
            }
        }
    }

    // ฟังก์ชันนี้ลดค่า Digestion ไปเรื่อยๆ ตราบใดที่ยังยืนอยู่ในบ้าน
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.DecreaseDigestion(reduceAmount * Time.deltaTime);
            }
        }
    }

    // ถ้าเดินออกจากบ้าน ยกเลิกโหมดปลอดภัยทันที!
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("⚠️ ออกจาก Safe Zone แล้ว!");

            if (WatchingHourManager.instance != null)
            {
                WatchingHourManager.instance.isPlayerSafe = false;
            }
        }
    }
}