using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    private int maxHp;
    public int damageToPlayer = 10;

    // ⭐ เช็คว่าตัวนี้ตายไปหรือยัง จะได้ไม่รันโค้ดซ้ำ
    private bool isDead = false;

    // ==========================================
    // 👑 Boss Settings
    // ==========================================
    [Header("👑 Boss Settings")]
    public bool isBoss = false;
    public Slider bossHealthBar;
    public float showHealthBarDistance = 15f;
    public string endGameSceneName = "End";
    public AudioClip bossDieSound;
    private Transform playerTransform;

    [Header("Drop System (ไอเทม)")]
    public GameObject lootBoxPrefab;
    public float boxDestroyTime = 10f;

    [Header("Drop Money (เงิน Orbs)")]
    public int minMoneyDrop = 5;
    public int maxMoneyDrop = 20;

    [Header("Visual Feedback")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("Bleeding System")]
    public Color bleedFlashColor = Color.red;
    public GameObject bleedVisualEffect;
    public Vector2 bleedIconOffset = new Vector2(0.5f, 0.5f);
    public float bleedIconScale = 1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine bleedCoroutine;
    private Rigidbody2D rb;

    // ⭐ 1. เพิ่มฟังก์ชัน Awake เข้ามา เพื่อให้จำเลือดสูงสุดทันทีที่เกิด!
    void Awake()
    {
        maxHp = hp;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // (เอา maxHp = hp; ออกจากตรงนี้ เพราะย้ายไปรันใน Awake ด้านบนแล้ว)

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (bleedVisualEffect != null)
        {
            GameObject bleedClone = Instantiate(bleedVisualEffect);
            bleedClone.transform.SetParent(this.transform);
            bleedClone.transform.localPosition = new Vector3(bleedIconOffset.x, bleedIconOffset.y, 0f);
            bleedClone.transform.localScale = Vector3.one * bleedIconScale;
            bleedClone.SetActive(false);
            bleedVisualEffect = bleedClone;
        }
        if (isBoss)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.value = 1f;
                bossHealthBar.gameObject.SetActive(false);
            }
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    // ==========================================
    // ⭐ ฟังก์ชันสำหรับรีเซ็ตเลือด
    // ==========================================
    public void ResetHealth()
    {
        hp = maxHp; // ดึงค่าเลือดกลับมาเต็ม (ตอนนี้ maxHp จะไม่เป็น 0 แล้ว!)
        isDead = false; // ปลดล็อคสถานะการตาย เพื่อให้บอสกลับมาสู้ต่อได้

        // ถ้าเป็นบอส ให้สั่งอัปเดตหลอดเลือดให้เต็มหลอดด้วย
        if (isBoss && bossHealthBar != null)
        {
            bossHealthBar.value = 1f;
        }
    }
    // ==========================================

    void Update()
    {
        // ถ้าตายแล้ว ไม่ต้องโชว์หลอดเลือด
        if (isDead) return;

        if (isBoss && playerTransform != null && bossHealthBar != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            bossHealthBar.gameObject.SetActive(distance <= showHealthBarDistance);
        }
    }

    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        // ถ้าตายแล้ว ไม่ต้องรับดาเมจอีก
        if (isDead) return;

        hp -= damage;
        Debug.Log(name + " เลือดเหลือ " + hp);

        if (isBoss && bossHealthBar != null)
        {
            bossHealthBar.value = (float)hp / maxHp;
        }

        StartFlashEffect();

        if (rb != null && knockbackForce > 0)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // เช็คว่าเลือดหมดหรือยัง
        if (hp <= 0)
        {
            isDead = true; // ล็อคสถานะว่าตายแล้ว
            Die();
        }
    }

    // ระบบเลือดไหล
    public void ApplyBleed(float duration, float damagePerSec)
    {
        if (isDead) return;
        if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);
        bleedCoroutine = StartCoroutine(BleedRoutine(duration, damagePerSec));
    }

    private System.Collections.IEnumerator BleedRoutine(float duration, float damagePerSec)
    {
        if (bleedVisualEffect != null) bleedVisualEffect.SetActive(true);
        float timer = 0f;
        while (timer < duration && !isDead)
        {
            yield return new WaitForSeconds(1f);
            TakeDamage((int)damagePerSec, 0f, Vector2.zero);
            timer += 1f;
        }
        if (bleedVisualEffect != null) bleedVisualEffect.SetActive(false);
        bleedCoroutine = null;
    }

    void StartFlashEffect()
    {
        if (spriteRenderer == null || isDead) return;
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
        if (isBoss)
        {
            Debug.Log("👑 บอสตายแล้ว! รอ 5 วินาที...");
            // เรียกฟังก์ชันหน่วงเวลา 5 วิ
            StartCoroutine(BossDeathRoutine());
            return;
        }

        // สำหรับมอนสเตอร์ทั่วไป
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

    IEnumerator BossDeathRoutine()
    {
        Debug.Log("👑 บอสตายแล้ว! กำลังหยุดสคริปต์โจมตีและรอวาร์ป...");

        // 1. สั่งปิดสคริปต์สุ่มโจมตี (BossAttackRandomizer) ทันที
        BossAttackRandomizer attackScript = GetComponent<BossAttackRandomizer>();
        if (attackScript != null)
        {
            attackScript.enabled = false; // ปิดการทำงานของสคริปต์
            attackScript.StopAllCoroutines(); // สั่งให้หยุด Coroutine การตีที่ค้างอยู่ทั้งหมด
        }

        // 2. ปิดภาพ (Sprite) ทั้งตัวแม่และตัวลูก
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in allSprites) sr.enabled = false;

        // 3. ปิดการชน (Collider) ทั้งตัวแม่และตัวลูก
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders) col.enabled = false;

        // 4. หยุดฟิสิกส์และหลอดเลือด
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);
        if (bossDieSound != null) AudioSource.PlayClipAtPoint(bossDieSound, transform.position);

        // ⏳ 5. รอนับถอยหลัง 5 วินาที
        yield return new WaitForSeconds(5f);

        // 6. วาร์ปไปหน้าจบเกม
        SceneManager.LoadScene(endGameSceneName);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return; // ถ้าตายแล้ว ไม่ต้องทำดาเมจใส่ผู้เล่น

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerTakeDamage(damageToPlayer);
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                player.ApplyKnockback(knockbackDir * 10f);
            }
        }
    }
}