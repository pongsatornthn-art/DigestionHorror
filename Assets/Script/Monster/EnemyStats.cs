using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    public int damageToPlayer = 10;

    [Header("Drop System (ไอเทม)")]
    public GameObject lootBoxPrefab;
    public float boxDestroyTime = 10f;
    public int minMoneyDrop = 5;
    public int maxMoneyDrop = 20;

    [Header("Visual Feedback (ตอนโดนตี)")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("Bleeding System (ระบบเลือดไหล)")]
    public Color bleedFlashColor = Color.red;
    public GameObject bleedVisualEffect; // ลาก GameObject ภาพหยดเลือดมาใส่ช่องนี้

    // ⭐ ส่วนที่เพิ่มเข้ามาใหม่สำหรับจัดตำแหน่งไอคอนเลือด
    [Header("Bleed Icon Settings (ตั้งค่าไอคอน)")]
    public Vector2 bleedIconOffset = new Vector2(0.5f, 0.5f); // ระยะห่างจากจุดศูนย์กลางมอนสเตอร์ (X, Y)
    public float bleedIconScale = 1f; // ปรับขนาดไอคอนให้เล็ก/ใหญ่

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine bleedCoroutine;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // ⭐ โค้ดใหม่: สร้างโคลนภาพเลือดตอนมอนสเตอร์เกิด
        if (bleedVisualEffect != null)
        {
            // 1. สร้างตัวโคลน (Clone) จากไฟล์ Prefab ภาพเลือด
            GameObject bleedClone = Instantiate(bleedVisualEffect);

            // 2. จับตัวโคลนมาเป็นลูกของมอนสเตอร์ตัวนี้
            bleedClone.transform.SetParent(this.transform);

            // 3. จัดตำแหน่งและขนาด
            bleedClone.transform.localPosition = new Vector3(bleedIconOffset.x, bleedIconOffset.y, 0f);
            bleedClone.transform.localScale = Vector3.one * bleedIconScale;

            // 4. ซ่อนไว้ก่อนรอโดนตี
            bleedClone.SetActive(false);

            // 5. อัปเดตให้ตัวแปรไปจำ "ตัวโคลน" แทนไฟล์ต้นฉบับ
            bleedVisualEffect = bleedClone;
        }
    }

    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        hp -= damage;
        Debug.Log(name + " โดนตี! เลือดเหลือ " + hp);

        StartFlashEffect();

        if (rb != null && knockbackForce > 0)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        if (hp <= 0) Die();
    }

    // ==========================================
    // ⭐ ระบบเลือดไหล (Bleeding)
    // ==========================================
    public void ApplyBleed(float duration, int damagePerSec)
    {
        if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);
        bleedCoroutine = StartCoroutine(BleedRoutine(duration, damagePerSec));
    }

    IEnumerator BleedRoutine(float duration, int damagePerSec)
    {
        float elapsed = 0f;

        // ⭐ เปิดภาพไอคอนเลือดข้างๆ มอนสเตอร์
        if (bleedVisualEffect != null) bleedVisualEffect.SetActive(true);

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;

            hp -= damagePerSec;
            Debug.Log($"🩸 {name} เลือดไหล! ลดไป {damagePerSec} (เหลือ {hp})");

            if (spriteRenderer != null)
            {
                spriteRenderer.color = bleedFlashColor;
                yield return new WaitForSeconds(0.15f);
                spriteRenderer.color = originalColor;
            }

            if (hp <= 0)
            {
                Die();
                yield break;
            }
        }

        // ⭐ เลือดหยุดไหลแล้ว ปิดภาพไอคอนเลือด
        if (bleedVisualEffect != null) bleedVisualEffect.SetActive(false);
        bleedCoroutine = null;
    }
    // ==========================================

    void StartFlashEffect()
    {
        if (spriteRenderer == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
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
        if (lootBoxPrefab != null)
        {
            GameObject droppedBox = Instantiate(lootBoxPrefab, transform.position, Quaternion.identity);
            Destroy(droppedBox, boxDestroyTime);
        }

        if (PlayerController.instance != null)
        {
            int droppedMoney = Random.Range(minMoneyDrop, maxMoneyDrop + 1);
            if (droppedMoney > 0)
            {
                PlayerController.instance.currentMoney += droppedMoney;
                PlayerController.instance.UpdateUI();
            }
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