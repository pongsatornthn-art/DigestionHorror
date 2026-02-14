using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Settings")]
    public float reduceAmount = 5f; // ค่า Digestion จะลดลงวินาทีละเท่าไหร่

    private void OnTriggerStay2D(Collider2D other)
    {
        // 1. เช็ค Tag ของวัตถุที่เข้ามาชน
        if (other.CompareTag("Player"))
        {
            // พิมพ์ข้อความเช็คใน Console
            Debug.Log("กำลังลดค่า Digestion ใน Safe Zone...");

            if (DigestionSystem.instance != null)
            {
                // 2. เรียกใช้ฟังก์ชันลดค่าที่เราสร้างไว้ใน DigestionSystem
                DigestionSystem.instance.DecreaseDigestion(reduceAmount * Time.deltaTime);

                // 3. ปิดระบบเพิ่มค่าอัตโนมัติ (ถ้ามี)
                DigestionSystem.instance.isWatchingHour = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ออกจาก Safe Zone แล้ว");
        }
    }
}