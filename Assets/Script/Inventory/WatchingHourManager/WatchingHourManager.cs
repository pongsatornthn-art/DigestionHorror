using UnityEngine;
using UnityEngine.UI; // ⭐ เรียกใช้ระบบ UI แทนแสง

public class WatchingHourManager : MonoBehaviour
{
    public static WatchingHourManager instance;

    [Header("สถานะปัจจุบัน")]
    public bool isPlayerSafe = false;

    [Header("การตั้งค่าเวลา (วินาที)")]
    public float minTimeBetweenEvents = 120f;
    public float maxTimeBetweenEvents = 300f;
    public float warningDuration = 10f;
    public float eventDuration = 60f;
    private float timer = 0f;

    enum EventState { Normal, Warning, Active }
    EventState currentState = EventState.Normal;

    [Header("ระบบภาพและเสียง")]
    public Image edgeEffectUI; // ⭐ ลาก UI ภาพขอบจอ (เช่น Vignette) มาใส่ตรงนี้

    public Color effectColor = Color.black; // สีของเอฟเฟกต์จอ (เปลี่ยนเป็นสีดำเพื่อให้จอมืดลง)

    [Range(0f, 1f)] public float maxAlpha = 0.8f; // ความเข้มสูงสุดของสีดำตอนเกิดเหตุ (1 คือมืดสนิท 0 คือใส)
    public AudioSource sirenAudio;

    [Header("บทลงโทษ (ต่อวินาที)")]
    public float digestionIncreasePerSec = 2f;

    void Awake() => instance = this;

    void Start()
    {
        timer = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);

        // ซ่อนขอบจอตอนเริ่มเกม
        if (edgeEffectUI != null)
        {
            Color c = effectColor;
            c.a = 0f;
            edgeEffectUI.color = c;
            edgeEffectUI.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case EventState.Normal:
                timer -= Time.deltaTime;
                if (timer <= 0) StartWarning();
                break;

            case EventState.Warning:
                timer -= Time.deltaTime;

                // ⭐ ค่อยๆ เฟดขอบจอให้มืดขึ้นเรื่อยๆ
                if (edgeEffectUI != null)
                {
                    Color c = effectColor;
                    c.a = Mathf.Lerp(maxAlpha, 0f, timer / warningDuration);
                    edgeEffectUI.color = c;
                }

                if (timer <= 0) StartEvent();
                break;

            case EventState.Active:
                timer -= Time.deltaTime;

                if (!isPlayerSafe)
                {
                    ApplyDamageToPlayer();
                }

                if (timer <= 0) EndEvent();
                break;
        }
    }

    // ==========================================
    // ⭐ [เพิ่มใหม่] ฟังก์ชันสำหรับให้ระบบเควสเรียกใช้โดยเฉพาะ!
    // ==========================================
    public void TriggerWatchingHourNow()
    {
        Debug.Log("เนื้อเรื่องสั่งเริ่ม Watching Hour ทันที!");
        StartWarning(); // บังคับเข้าสู่ช่วงเตือนภัยทันที ข้ามเวลารอไปเลย
    }

    public void ForceStopWatchingHour()
    {
        Debug.Log("เนื้อเรื่องสั่งหยุด Watching Hour ทันที!");
        EndEvent(); // บังคับให้เหตุการณ์สงบลง (เผื่อเอาไว้ใช้ตอนส่งเควสเสร็จ)
    }
    // ==========================================

    // ⭐ เปลี่ยนเป็น public เผื่ออยากให้ Event เรียกใช้ตรงๆ
    public void StartWarning()
    {
        currentState = EventState.Warning;
        timer = warningDuration;

        if (edgeEffectUI != null)
        {
            edgeEffectUI.gameObject.SetActive(true);
            Color c = effectColor;
            c.a = 0f;
            edgeEffectUI.color = c;
        }

        Debug.Log("⚠️ The Watching Hour กำลังจะมา! รีบหาที่ซ่อน!");
    }

    public void StartEvent()
    {
        currentState = EventState.Active;
        timer = eventDuration;

        if (edgeEffectUI != null)
        {
            Color c = effectColor;
            c.a = maxAlpha;
            edgeEffectUI.color = c;
        }

        if (sirenAudio != null) sirenAudio.Play();
        if (DigestionSystem.instance != null) DigestionSystem.instance.isWatchingHour = true;

        Debug.Log("💀 The Watching Hour เริ่มต้นขึ้นแล้ว! จอมืดลง!");
    }

    public void EndEvent()
    {
        currentState = EventState.Normal;
        timer = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);

        // ซ่อนขอบจอ (กลับมาสว่างปกติ)
        if (edgeEffectUI != null) edgeEffectUI.gameObject.SetActive(false);

        if (sirenAudio != null) sirenAudio.Stop();
        if (DigestionSystem.instance != null) DigestionSystem.instance.isWatchingHour = false;

        Debug.Log("☀️ เหตุการณ์สงบลงแล้ว...");
    }

    void ApplyDamageToPlayer()
    {
        if (DigestionSystem.instance != null)
        {
            DigestionSystem.instance.IncreaseDigestion(digestionIncreasePerSec * Time.deltaTime);
        }
    }
}