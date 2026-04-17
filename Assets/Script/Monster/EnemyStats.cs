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

    // ==========================================
    // ⭐ ระบบเลือดไหล (Bleeding) ของเพื่อน
    // ==========================================
    [Header("Bleeding System (ระบบเลือดไหล)")]
    public Color bleedFlashColor = Color.red;
    public GameObject bleedVisualEffect; // ลาก GameObject ภาพหยดเลือดมาใส่ช่องนี้

    [Header("Bleed Icon Settings (ตั้งค่าไอคอน)")]
    public Vector2 bleedIconOffset = new Vector2(0.5f, 0.5f); // ระยะห่างจากจุดศูนย์กลางมอนสเตอร์
    public float bleedIconScale = 1f; // ปรับขนาดไอคอน

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
        else
        {
            Debug.LogError(name + " ไม่มี SpriteRenderer แปะอยู่! ระบบตัวขาวจะทำงานไม่ได้");
        }

        // ⭐ โค้ดสร้างโคลนภาพเลือดตอนมอนสเตอร์เกิด
        if (bleedVisualEffect != null)
        {
            GameObject bleedClone = Instantiate(bleedVisualEffect);
            bleedClone.transform.SetParent(this.transform);
            bleedClone.transform.localPosition = new Vector3(bleedIconOffset.x, bleedIconOffset.y, 0f);
            bleedClone.transform.localScale = Vector3.one * bleedIconScale;
            bleedClone.SetActive(false);
            bleedVisualEffect = bleedClone; // จำตัวโคลนไว้ใช้แทน
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

    // ==========================================
    // ⭐ ฟังก์ชันติดสถานะเลือดไหล (โดนเรียกจาก WeaponAttack)
    // ==========================================
    public void ApplyBleed(float duration, float damagePerSec)
    {
        if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);
        bleedCoroutine = StartCoroutine(BleedRoutine(duration, damagePerSec));
    }

    private System.Collections.IEnumerator BleedRoutine(float duration, float damagePerSec)
    {
        if (bleedVisualEffect != null) bleedVisualEffect.SetActive(true);

        float timer = 0f;
        while (timer < duration)
        {
            yield return new WaitForSeconds(1f);
            // สั่งลดเลือด (แปลง float เป็น int) แบบไม่กระเด็น
            TakeDamage((int)damagePerSec, 0f, Vector2.zero);
            timer += 1f;
        }

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
        // 1. ดรอปกล่องไอเทม
        if (lootBoxPrefab != null)
        {
            GameObject droppedBox = Instantiate(lootBoxPrefab, transform.position, Quaternion.identity);
            Debug.Log(name + " ดรอปกล่องแล้ว!");
            Destroy(droppedBox, boxDestroyTime);
        }

        // 2. ดรอปเงิน (Orbs) เข้าตัวผู้เล่น
        if (PlayerController.instance != null)
        {
            int droppedMoney = Random.Range(minMoneyDrop, maxMoneyDrop + 1);

            if (droppedMoney > 0)
            {
                PlayerController.instance.currentMoney += droppedMoney;
                PlayerController.instance.UpdateUI();
                Debug.Log($"✨ {name} ตาย! ได้รับเงิน (Orbs): {droppedMoney} | เงินรวม: {PlayerController.instance.currentMoney}");
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