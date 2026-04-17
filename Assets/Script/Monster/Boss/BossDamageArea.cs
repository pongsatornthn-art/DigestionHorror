using UnityEngine;

public class BossDamageArea : MonoBehaviour
{
    public int damageAmount = 30; // ตั้งค่าใน Inspector (ซ้าย 30, ขวา 30)
    public float knockbackForce = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // เช็คว่าชน Player หรือไม่
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
                // ถ้า Player ไม่ได้อยู่ในสถานะอมตะ (isInvulnerable)
                if (!player.isInvulnerable)
                {
                    // ทำดาเมจ
                    player.PlayerTakeDamage(damageAmount);

                    // ใส่ Knockback (ใช้ระบบที่มีอยู่ใน PlayerController.cs ของคุณ)
                    Vector2 dir = (player.transform.position - transform.position).normalized;
                    player.ApplyKnockback(dir * knockbackForce);

                    Debug.Log(gameObject.name + " hit Player for " + damageAmount + " damage!");
                }
            }
        }
    }
}