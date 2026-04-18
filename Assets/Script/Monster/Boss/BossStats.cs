using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // สำคัญมากสำหรับการเปลี่ยนฉาก

public class BossStats : MonoBehaviour
{
    public int maxHealth = 70;
    private int currentHealth;

    [Header("UI")]
    public Slider healthBar;
    public float showHealthBarDistance = 15f;

    private Transform playerTransform;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip dieSound;

    [Header("End Game Settings")]
    public string endGameSceneName = "EndingScene"; // ใส่ชื่อซีนฉากจบที่นี่

    void Start()
    {
        currentHealth = maxHealth;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (healthBar != null) healthBar.gameObject.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (playerTransform != null && healthBar != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            healthBar.gameObject.SetActive(distance <= showHealthBarDistance);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (hitSound != null && audioSource != null) audioSource.PlayOneShot(hitSound);
        UpdateUI();

        // ⭐ เช็คเงื่อนไขตามที่คุณพงศธรแนะนำ: เลือดน้อยกว่าหรือเท่ากับ 0
        if (currentHealth <= 0)
        {
            Debug.Log("บอสตายแล้ว! กำลังเตรียมวาร์ปไปฉาก: " + endGameSceneName);
            WarpToEnding();
        }
    }

    void WarpToEnding()
    {
        // 1. เล่นเสียงตายแบบ 3D (ใช้ PlayClipAtPoint เพื่อให้เสียงยังดังอยู่แม้บอสจะหายไป)
        if (dieSound != null) AudioSource.PlayClipAtPoint(dieSound, transform.position);

        // 2. สั่งวาร์ปทันที (ไม่ต้องรอ Destroy)
        // การเปลี่ยนฉากจะเคลียร์ Object ทั้งหมดในฉากเก่าออกเองอัตโนมัติครับ
        SceneManager.LoadScene(endGameSceneName);
    }

    void UpdateUI()
    {
        if (healthBar != null) healthBar.value = (float)currentHealth / maxHealth;
    }
}