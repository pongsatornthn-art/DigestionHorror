using UnityEngine;
using System.Collections; // ⭐ จำเป็นต้องเพิ่มบรรทัดนี้เพื่อใช้ IEnumerator (การจับเวลา)

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    public int damageToPlayer = 10;

    [Header("Drop System")]
    public GameObject lootBoxPrefab;
    public float boxDestroyTime = 10f;

    // ==========================================
    // ⭐ ส่วนที่เพิ่มใหม่: ระบบ Hit Flash Effect (ตัวขาว)
    // ==========================================
    [Header("Visual Feedback (ตอนโดนตี)")]
    public Color flashColor = Color.white; // สีที่จะให้กระพริบ (ค่าเริ่มต้นคือขาว)
    public float flashDuration = 0.1f;    // ระยะเวลาที่ตัวเป็นสีขาว (วินาที)

    private SpriteRenderer spriteRenderer;  // ตัวเก็บคอมโพเนนต์แสดงภาพ
    private Color originalColor;           // ตัวเก็บสีดั้งเดิมของศัตรู
    private Coroutine flashCoroutine;       // ตัวคุม Coroutine ไม่ให้ตีกัน

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ⭐ ดึงคอมโพเนนต์ SpriteRenderer มาเก็บไว้ตอนเริ่มเกม
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ถ้าเจอ SpriteRenderer ให้เก็บสีดั้งเดิมไว้
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            // ปริ้นเตือนเผื่อคุณลืมแปะ SpriteRenderer ไว้ที่ตัวมอนสเตอร์
            Debug.LogError(name + " ไม่มี SpriteRenderer แปะอยู่! ระบบตัวขาวจะทำงานไม่ได้");
        }
    }

    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        hp -= damage;
        Debug.Log(name + " เลือดเหลือ " + hp);

        // ⭐ สั่งให้ตัวกระพริบเป็นสีขาว!
        StartFlashEffect();

        if (rb != null && knockbackForce > 0)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (hp <= 0) Die();
    }

    // ==========================================
    // ⭐ ส่วนฟังก์ชันคุมการกระพริบ
    // ==========================================

    // ฟังก์ชันหลักที่ถูกเรียกตอนโดนตี
    void StartFlashEffect()
    {
        if (spriteRenderer == null) return; // ถ้าไม่มีภาพ ไม่ต้องทำอะไร

        // ถ้ามันกำลังกระพริบอยู่ ให้หยุดอันเก่าก่อน แล้วเริ่มนับ 1 ใหม่
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        // เริ่มต้นการนับเวลาเปลี่ยนสี (Coroutine)
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    // Coroutine: ฟังก์ชันพิเศษที่สามารถ "หยุดรอ" เวลาได้
    IEnumerator FlashRoutine()
    {
        // 1. เปลี่ยนสี Sprite เป็นสีขาว (flashColor)
        spriteRenderer.color = flashColor;

        // 2. หยุดรอเวลาตามที่กำหนดไว้ (flashDuration)
        yield return new WaitForSeconds(flashDuration);

        // 3. เปลี่ยนสีกลับเป็นสีดั้งเดิม
        spriteRenderer.color = originalColor;

        // เคลียร์ค่าตัวคุม Coroutine
        flashCoroutine = null;
    }

    // ==========================================

    void Die()
    {
        if (lootBoxPrefab != null)
        {
            GameObject droppedBox = Instantiate(lootBoxPrefab, transform.position, Quaternion.identity);
            Debug.Log(name + " ดรอปกล่องแล้ว!");
            Destroy(droppedBox, boxDestroyTime);
        }

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

                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                float force = 10f;
                player.ApplyKnockback(knockbackDir * force);
            }
        }
    }
}