using UnityEngine;

public class DigestionSystem : MonoBehaviour
{
    public static DigestionSystem instance;

    [Header("Digestion Stats")]
    public float maxDigestion = 100f;
    public float currentDigestion = 0f;

    [HideInInspector] public bool isWatchingHour = false;

    // ==========================================
    // ⭐ ภาพรอยเลือด 4 ระดับ (ชื่อตรงกับออบเจกต์ใน Unity เป๊ะๆ)
    // ==========================================
    [Header("Blood Screen Stages")]
    public GameObject blood25;  // สำหรับ 10% - 25% (ลากออบเจกต์ '25' มาใส่)
    public GameObject blood50;  // สำหรับ 25% - 50% (ลากออบเจกต์ '50' มาใส่)
    public GameObject blood75;  // สำหรับ 50% - 75% (ลากออบเจกต์ '75' มาใส่)
    public GameObject blood100; // สำหรับ 75% - 100% (ลากออบเจกต์ '100' มาใส่)

    // --- ระบบคำนวณ Totem ---
    private float currentDigestionMultiplier = 1f;
    private float totemTimer = 0f;
    [HideInInspector] public bool isTotemActive = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateBloodScreen(); // อัปเดตภาพตั้งแต่เริ่มเกม
    }

    void Update()
    {
        // จัดการเวลานับถอยหลังของโทเทม
        if (isTotemActive)
        {
            totemTimer -= Time.deltaTime;
            if (totemTimer <= 0)
            {
                isTotemActive = false;
                currentDigestionMultiplier = 1f;
            }
        }
    }

    public void IncreaseDigestion(float amount)
    {
        currentDigestion += (amount * currentDigestionMultiplier);
        currentDigestion = Mathf.Clamp(currentDigestion, 0, maxDigestion);
        UpdateBloodScreen(); // โดนดาเมจปุ๊บ สั่งเช็คภาพเลือดปั๊บ
    }

    public void DecreaseDigestion(float amount)
    {
        currentDigestion -= amount;
        currentDigestion = Mathf.Clamp(currentDigestion, 0, maxDigestion);
        UpdateBloodScreen(); // ฮีลปุ๊บ สั่งเช็คภาพเลือดปั๊บ
    }

    public void ApplyTotemBuff(float multiplier, float duration)
    {
        currentDigestionMultiplier = multiplier;
        totemTimer = duration;
        isTotemActive = true;
    }

    // ==========================================
    // ⭐ ฟังก์ชันเปิด-ปิดภาพ (อ้างอิงตามรูปวาดเป๊ะๆ 100%)
    // ==========================================
    private void UpdateBloodScreen()
    {
        // แปลงค่าหลอดเลือดให้เป็นเปอร์เซ็นต์ (0 - 100)
        float percent = (currentDigestion / maxDigestion) * 100f;

        // 1. สั่งปิดทุกรูปก่อนกันภาพทับซ้อนกัน
        if (blood25 != null) blood25.SetActive(false);
        if (blood50 != null) blood50.SetActive(false);
        if (blood75 != null) blood75.SetActive(false);
        if (blood100 != null) blood100.SetActive(false);

        // 2. เช็คเงื่อนไขตามรูปสมุดจดเป๊ะๆ!
        if (percent >= 75f && percent <= 100f)
        {
            // Digestion ระดับ 75% - 100%
            if (blood100 != null) blood100.SetActive(true);
        }
        else if (percent >= 50f && percent < 75f)
        {
            // Digestion ระดับ 50% - 75%
            if (blood75 != null) blood75.SetActive(true);
        }
        else if (percent >= 25f && percent < 50f)
        {
            // Digestion ระดับ 25% - 50%
            if (blood50 != null) blood50.SetActive(true);
        }
        else if (percent >= 10f && percent < 25f)
        {
            // Digestion ระดับ 10% - 25%
            if (blood25 != null) blood25.SetActive(true);
        }
        // ต่ำกว่า 10% คือหน้าจอใสสะอาดปกติครับ
    }

    // ==========================================
    // ส่วนส่งออกข้อมูล (เผื่อระบบอื่นอยากรู้)
    // ==========================================
    public float GetDigestionRatio() => currentDigestion / maxDigestion;
    public float GetTotemTimeLeft() => totemTimer;

    public int GetSpawnBonus()
    {
        float ratio = GetDigestionRatio();
        if (ratio >= 1.0f) return 7;
        if (ratio >= 0.75f) return 4;
        if (ratio >= 0.5f) return 2;
        return 0;
    }
}