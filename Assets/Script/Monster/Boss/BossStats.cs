using UnityEngine;
using UnityEngine.UI;

public class BossStats : MonoBehaviour
{
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("UI")]
    public Slider healthBar;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;  // ใส่ไฟล์ ทุบเนือ.mp3
    [SerializeField] private AudioClip dieSound;  // ใส่ไฟล์ เสียงร้องก่อนตายBOSS.mp3

    void Start()
    {
        currentHealth = maxHealth;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // เล่นเสียงเมื่อโดนโจมตี
        if (hitSound != null) audioSource.PlayOneShot(hitSound);

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // เล่นเสียงตายก่อนทำลาย Object
        if (dieSound != null) AudioSource.PlayClipAtPoint(dieSound, transform.position);
        Destroy(gameObject);
    }

    void UpdateUI()
    {
        if (healthBar != null) healthBar.value = (float)currentHealth / maxHealth;
    }
}