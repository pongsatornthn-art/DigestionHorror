using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("ตั้งค่าฟื้นฟู")]
    public float reduceAmountPerSec = 5f;

    private bool isPlayerInside = false;

    void Update()
    {
        if (isPlayerInside)
        {
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.DecreaseDigestion(reduceAmountPerSec * Time.deltaTime);

                // ⭐ โค้ดจับโกหก: มันจะปริ้นบอกรัวๆ ว่ากำลังลดค่าของออบเจกต์ชื่ออะไรอยู่!
                Debug.Log($"[SafeZone] กำลังลดค่า! ตอนนี้ Digestion เหลือ: {DigestionSystem.instance.currentDigestion:F1} (ลดที่ออบเจกต์ชื่อ: {DigestionSystem.instance.gameObject.name})");
            }
            else
            {
                Debug.LogError("[SafeZone] พังแล้ว! หา DigestionSystem.instance ไม่เจอ!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log("[SafeZone] สวิตช์เปิด: ผู้เล่นเดินเข้าบ้านแล้ว!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log("[SafeZone] สวิตช์ปิด: ผู้เล่นเดินออกแล้ว!");
        }
    }
}