using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    public int damageToPlayer = 10;

    [Header("Drop System (ไอเทม)")]
    public GameObject lootBoxPrefab;
    public float boxDestroyTime = 10f;

    // ==========================================
    // ⭐ ส่วนที่เพิ่มใหม่: ระบบดรอปเงิน (Orbs)
    // ==========================================
    [Header("Drop Money (เงิน Orbs)")]
    public int minMoneyDrop = 5;  // สุ่มดรอปขั้นต่ำ
    public int maxMoneyDrop = 20; // สุ่มดรอปสูงสุด

    [Header("Visual Feedback (ตอนโดนตี)")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError(name + " ไม่มี SpriteRenderer แปะอยู่! ระบบตัวขาวจะทำงานไม่ได้");
        }
    }

    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        hp -= damage;
        Debug.Log(name + " เลือดเหลือ " + hp);

        StartFlashEffect();

        if (rb != null && knockbackForce > 0)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (hp <= 0) Die();
    }

    void StartFlashEffect()
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    void Die()
    {
        // 1. ดรอปกล่องไอเทม (ถ้ามี)
        if (lootBoxPrefab != null)
        {
            GameObject droppedBox = Instantiate(lootBoxPrefab, transform.position, Quaternion.identity);
            Debug.Log(name + " ดรอปกล่องแล้ว!");
            Destroy(droppedBox, boxDestroyTime);
        }

        // 2. ⭐ ดรอปเงิน (Orbs) เข้าตัวผู้เล่นโดยตรง
        if (PlayerController.instance != null)
        {
            // สุ่มจำนวนเงิน (+1 ข้างหลังเพราะสูตรสุ่มมันจะไม่นับเลขตัวสุดท้ายครับ)
            int droppedMoney = Random.Range(minMoneyDrop, maxMoneyDrop + 1);

            if (droppedMoney > 0)
            {
                PlayerController.instance.currentMoney += droppedMoney; // เพิ่มเงิน
                PlayerController.instance.UpdateUI(); // สั่งให้หน้าจออัปเดตตัวเลขทันที
                Debug.Log($"✨ {name} ตาย! ได้รับเงิน (Orbs): {droppedMoney} | เงินรวม: {PlayerController.instance.currentMoney}");
            }
        }

        Destroy(gameObject); // ทำลายศัตรูทิ้ง
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerTakeDamage(damageToPlayer);

                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                float force = 10f;
                player.ApplyKnockback(knockbackDir * force);
            }
        }
    }
}