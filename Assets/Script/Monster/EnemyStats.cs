using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    public int damageToPlayer = 10;

    [Header("Drop System")]
    public GameObject lootBoxPrefab; // ช่องสำหรับลาก Prefab กล่องมาใส่
    public float boxDestroyTime = 10f; // ตั้งเวลาให้กล่องหายไป (ค่าเริ่มต้น 10 วินาที)

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        hp -= damage;
        Debug.Log(name + " เลือดเหลือ " + hp);

        if (rb != null && knockbackForce > 0)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (hp <= 0) Die();
    }

    void Die()
    {
        if (lootBoxPrefab != null)
        {
            // 1. สร้างกล่องออกมา และเก็บข้อมูลกล่องใบนั้นไว้ในตัวแปร droppedBox
            GameObject droppedBox = Instantiate(lootBoxPrefab, transform.position, Quaternion.identity);
            Debug.Log(name + " ดรอปกล่องแล้ว!");

            // 2. สั่งทำลายกล่องใบนั้นทิ้ง หลังจากเวลาผ่านไปตามที่กำหนด (10 วินาที)
            Destroy(droppedBox, boxDestroyTime);
        }

        Destroy(gameObject); // ทำลายตัวมอนสเตอร์ทิ้ง
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