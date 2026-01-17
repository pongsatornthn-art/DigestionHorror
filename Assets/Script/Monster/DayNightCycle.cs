using UnityEngine;
using UnityEngine.Rendering.Universal; // ต้องมีบรรทัดนี้สำหรับแสง 2D

public class DayNightCycle : MonoBehaviour
{
    public Light2D globalLight; // ลาก Global Light มาใส่
    public float dayDuration = 60f; // 1 วัน = 60 วินาที

    private float time;

    void Update()
    {
        time += Time.deltaTime;

        // คำนวณค่าสีของแสงตามเวลา (Sine Wave)
        // กลางวัน = 1 (สว่าง), กลางคืน = 0 (มืด)
        float intensity = (Mathf.Sin(time / dayDuration * Mathf.PI * 2) + 1) / 2;

        // ปรับแสงอย่าให้มืดสนิท (เหลือไว้สัก 0.1)
        globalLight.intensity = Mathf.Lerp(0f, 1f, intensity);
    }
}