using UnityEngine;
using UnityEngine.UI;
using TMPro; // ⭐ เพิ่มเข้ามาเพื่อใช้ TextMeshPro

public class DigestionSystem : MonoBehaviour
{
    public static DigestionSystem instance;

    [Header("Digestion Stats")]
    public float maxDigestion = 100f;
    public float currentDigestion = 0f;

    [Header("The Watching Hour Settings")]
    public bool isWatchingHour = false;
    public float watchingHourRate = 2f;

    [Header("UI")]
    public Slider digestionSlider;

    // ตัวแปรสำหรับคุมความเร็วการย่อยอาหาร
    private float currentDigestionMultiplier = 1f;

    // ==========================================
    // ⭐ ส่วนที่เพิ่มใหม่: Totem Timer & UI
    // ==========================================
    [Header("Totem Buff UI")]
    public GameObject totemUIContainer; // กรอบ UI หลักของโทเทม (เอาไว้สั่ง เปิด/ปิด)
    public TextMeshProUGUI totemTimeText; // ตัวหนังสือบอกเวลาถอยหลัง
    public Slider totemTimeSlider; // (ตัวเลือกเสริม: หลอดเวลา ลดลงเรื่อยๆ ใส่หรือไม่ใส่ก็ได้)

    private bool isTotemActive = false;
    private float totemTimer = 0f;
    private float totemMaxDuration = 0f;

    void Awake() => instance = this;

    void Start()
    {
        // เริ่มเกมมา ให้ซ่อน UI โทเทมไว้ก่อน
        if (totemUIContainer != null) totemUIContainer.SetActive(false);
    }

    void Update()
    {
        if (isWatchingHour)
        {
            IncreaseDigestion(watchingHourRate * currentDigestionMultiplier * Time.deltaTime);
        }

        // ⭐ จัดการนับเวลาถอยหลัง Totem และอัปเดต UI ทุกๆ เฟรม
        if (isTotemActive)
        {
            totemTimer -= Time.deltaTime; // เวลาน้อยลงเรื่อยๆ

            // อัปเดตตัวเลข (Mathf.CeilToInt จะปัดเศษขึ้น จะได้ไม่มีทศนิยมยาวๆ)
            if (totemTimeText != null)
            {
                totemTimeText.text = Mathf.CeilToInt(totemTimer).ToString() + "s";
            }

            // อัปเดตหลอดเวลา (ถ้าคุณอยากทำเป็นหลอดค่อยๆ ลด)
            if (totemTimeSlider != null)
            {
                totemTimeSlider.value = totemTimer / totemMaxDuration;
            }

            // เช็คว่าเวลาหมดหรือยัง
            if (totemTimer <= 0)
            {
                isTotemActive = false;
                currentDigestionMultiplier = 1f; // กลับมาเพิ่มความเร็วปกติ

                if (totemUIContainer != null) totemUIContainer.SetActive(false); // ซ่อน UI
                Debug.Log("หมดฤทธิ์โทเทมแล้ว! UI โทเทมหายไป");
            }
        }
    }

    public void IncreaseDigestion(float amount)
    {
        currentDigestion += amount;
        currentDigestion = Mathf.Clamp(currentDigestion, 0, maxDigestion);
        UpdateUI();
    }

    public void DecreaseDigestion(float amount)
    {
        currentDigestion -= amount;
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
        float ratio = currentDigestion / maxDigestion;

        if (ratio >= 1.0f) return 7;
        if (ratio >= 0.75f) return 4;
        if (ratio >= 0.5f) return 2;

        return 0;
    }

    // ==========================================
    // ⭐ ฟังก์ชันรับคำสั่งปักโทเทม
    // ==========================================
    public void ApplyTotemBuff(float multiplier, float duration)
    {
        currentDigestionMultiplier = multiplier;
        totemMaxDuration = duration;
        totemTimer = duration;
        isTotemActive = true;

        // สั่งเปิด UI โทเทมโชว์ขึ้นมาบนหน้าจอ
        if (totemUIContainer != null) totemUIContainer.SetActive(true);

        // บังคับอัปเดตตัวเลขทันทีในวินาทีแรกที่กดใช้
        if (totemTimeText != null) totemTimeText.text = duration.ToString() + "s";
        if (totemTimeSlider != null) totemTimeSlider.value = 1f;
    }
}