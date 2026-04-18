using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("ตั้งค่าดาเมจ")]
    public int damage = 20;

    // ทำงานตอนที่ผู้เล่นเพิ่งเดินเข้ามาโดนแขน
    void OnTriggerEnter2D(Collider2D other)
    {
        DealDamage(other);
    }

    // ⭐ ทำงานตอนที่ผู้เล่น "ยืนแช่" อยู่ แล้วบอสเปิดแขนมาทับพอดี
    void OnTriggerStay2D(Collider2D other)
    {
        DealDamage(other);
    }

    void DealDamage(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            // เช็คว่ามีตัวผู้เล่น และผู้เล่นไม่ได้กำลังติดสถานะอมตะ (กระพริบแดง) อยู่
            if (player != null && !player.isInvulnerable)
            {
                Debug.Log("💥 บอสฟาดโดนผู้เล่น! ลดเลือด: " + damage);
                player.PlayerTakeDamage(damage);
            }
        }
    }
}