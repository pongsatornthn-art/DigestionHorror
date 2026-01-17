using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ✅ ต้องเพิ่มบรรทัดนี้ เพื่อให้โหลดฉากใหม่ได้

public class DigestionSystem : MonoBehaviour
{
    public static DigestionSystem instance;

    [Header("ค่าการย่อยสลาย (HP)")]
    public float maxDigestion = 100f;
    public float currentDigestion = 0f;

    [Header("เงื่อนไข: อยู่นิ่งๆ")]
    public float timeBeforeDigest = 3f;
    public float digestionRate = 5f;
    public float moveThreshold = 0.1f;

    [Header("UI")]
    public Slider digestionSlider;
    public GameObject gameOverPanel; // ✅ ลากหน้าต่าง Game Over มาใส่ตรงนี้

    private float idleTimer = 0f;
    private Vector3 lastPosition;
    private bool isDead = false; // เช็คว่าตายหรือยัง

    void Awake() { instance = this; }

    void Start()
    {
        currentDigestion = 0;
        lastPosition = transform.position;
        UpdateUI();

        // ซ่อนหน้าต่าง Game Over ตอนเริ่มเกมเสมอ
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // มั่นใจว่าเวลาเดินปกติ (เผื่อตายรอบที่แล้วเวลาหยุดอยู่)
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isDead) return; // ถ้าตายแล้ว ไม่ต้องคำนวณอะไรต่อ

        CheckMovement();
    }

    void CheckMovement()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance > moveThreshold)
        {
            idleTimer = 0f;
            lastPosition = transform.position;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= timeBeforeDigest)
            {
                IncreaseDigestion(digestionRate * Time.deltaTime);
            }
        }
    }

    public void IncreaseDigestion(float amount)
    {
        if (isDead) return;

        currentDigestion += amount;
        currentDigestion = Mathf.Clamp(currentDigestion, 0, maxDigestion);

        UpdateUI();

        if (currentDigestion >= maxDigestion)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (digestionSlider != null)
        {
            digestionSlider.value = currentDigestion / maxDigestion;
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Game Over: คุณถูกหมู่บ้านกลืนกินแล้ว!");

        // 1. หยุดเวลาเกม (ทุกอย่างจะหยุดขยับ)
        Time.timeScale = 0f;

        // 2. เปิดหน้าต่าง Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // ✅ ฟังก์ชันนี้เอาไว้ใส่ในปุ่ม Restart
    public void RestartGame()
    {
        // คืนค่าเวลาให้เดินปกติ
        Time.timeScale = 1f;

        // โหลดฉากปัจจุบันใหม่ (เริ่มเกมใหม่)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}