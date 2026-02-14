using UnityEngine;
using UnityEngine.UI;

public class DigestionSystem : MonoBehaviour
{
    public static DigestionSystem instance;

    [Header("Digestion Stats")]
    public float maxDigestion = 100f;
    public float currentDigestion = 0f;

    [Header("The Watching Hour Settings")]
    public bool isWatchingHour = false; // สคริปต์อื่นมาสั่งเปิด-ปิดได้
    public float watchingHourRate = 2f;  // ค่าย่อยเพิ่มขึ้นวินาทีละเท่าไหร่ตอนอยู่นอกบ้าน

    [Header("UI")]
    public Slider digestionSlider;

    void Awake() => instance = this;

    void Update()
    {
        // 1. ถ้าเป็นช่วง Watching Hour และอยู่นอกบ้าน ค่า Digestion จะเพิ่มเรื่อยๆ
        if (isWatchingHour)
        {
            IncreaseDigestion(watchingHourRate * Time.deltaTime);
        }
    }

    // 2. ฟังก์ชันเพิ่มค่า Digestion (เรียกใช้เมื่อโดน Monster ตี หรืออยู่นอกบ้าน)
    public void IncreaseDigestion(float amount)
    {
        currentDigestion += amount;
        currentDigestion = Mathf.Clamp(currentDigestion, 0, maxDigestion);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (digestionSlider != null)
        {
            digestionSlider.value = currentDigestion / maxDigestion;
        }
    }

    public int GetSpawnBonus()
    {
        float ratio = currentDigestion / maxDigestion; // แปลงเป็น 0.0 - 1.0

        if (ratio >= 1.0f) return 7;   // Digestion 100% = +7 ตัว
        if (ratio >= 0.75f) return 4;  // Digestion 75% = +4 ตัว
        if (ratio >= 0.5f) return 2;   // Digestion 50% = +2 ตัว

        return 0; // ต่ำกว่า 50% ไม่เกิดเพิ่ม
    }
    public void DecreaseDigestion(float amount)
    {
        currentDigestion -= amount;
        currentDigestion = Mathf.Clamp(currentDigestion, 0, maxDigestion);
        UpdateUI();
    }
}