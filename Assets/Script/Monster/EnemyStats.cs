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
    public Color bleedFlashColor = Color.red; // สีตอนกระพริบเลือดไหล
    public GameObject bleedVisualEffect; // ⭐ ลาก GameObject ภาพเลือดมาใส่ช่องนี้!

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

        // เริ่มเกมมา บังคับซ่อนภาพเลือดไว้ก่อน
        if (bleedVisualEffect != null) bleedVisualEffect.SetActive(false);
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

        // ⭐ เปิดภาพเอฟเฟกต์เลือดบนหัวมอนสเตอร์
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

        // ⭐ เลือดหยุดไหลแล้ว ปิดภาพเอฟเฟกต์เลือด
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