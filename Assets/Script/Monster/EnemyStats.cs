using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    public int damageToPlayer = 10;

    private Rigidbody2D rb; // ✅ เพิ่มเพื่อใช้รับแรง Knockback

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ✅ ดึง Rigidbody2D ของมอนสเตอร์มาใช้
    }

    // ✅ ปรับให้รับค่า damage, แรงผลัก (force), และทิศทาง (direction)
    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        hp -= damage;
        Debug.Log(name + " เลือดเหลือ " + hp);

        // ✅ ถ้ามีแรงผลัก ให้มอนสเตอร์กระเด็นไปตามทิศทาง
        if (rb != null && knockbackForce > 0)
        {
            // ใช้ ForceMode2D.Impulse เพื่อให้เกิดแรงกระแทกทันที
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (hp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerTakeDamage(damageToPlayer);
            }
        }
    }
}