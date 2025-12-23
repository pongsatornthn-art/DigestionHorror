using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("ตั้งค่าเลือด")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Effect (Optional)")]
    // ต้องมีตัวแปรมารองรับ Header เสมอครับ
    public GameObject deathEffect; // (ตัวอย่าง) ใส่ Effect ระเบิดตอนตาย
    public AudioClip deathSound;   // (ตัวอย่าง) ใส่เสียงร้องตอนตาย

    void Start()
    {
        currentHealth = maxHealth; // เริ่มเกมเลือดเต็ม
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย Player เมื่อโดนโจมตี
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " โดนตี! เลือดเหลือ: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // อาจจะใส่ Effect ตอนโดนตีตรงนี้ (เช่น ตัวกระพริบสีขาว)
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ตายแล้ว!");
        // ในอนาคตอาจจะเปลี่ยนเป็นเล่น Animation ตาย หรือระเบิดเป็นเลือด
        Destroy(gameObject); // ทำลายตัวเองทิ้ง
    }
}