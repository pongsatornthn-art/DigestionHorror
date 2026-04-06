using UnityEngine;
using System.Collections;

public class MagicCircleTimer : MonoBehaviour
{
    [Header("การตั้งค่าเวลา")]
    public float stayDuration = 10f; // อยู่กี่วินาที
    private float timer;

    [Header("จุดวาร์ป (ลาก Empty Object มาใส่)")]
    public Transform[] spawnPoints;

    [Header("เอฟเฟกต์ตอนวาร์ป (ถ้ามี)")]
    public ParticleSystem teleportEffect;

    void Start()
    {
        timer = stayDuration;
        // เริ่มเกมมาให้สุ่มไปจุดแรกก่อนเลย
        TeleportToRandomPoint();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            TeleportToRandomPoint();
            timer = stayDuration; // รีเซ็ตเวลาใหม่
        }
    }

    public void TeleportToRandomPoint()
    {
        if (spawnPoints.Length == 0) return;

        // สุ่มเลือกจุดจาก Array
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Vector3 nextPosition = spawnPoints[randomIndex].position;

        // ถ้ามีเอฟเฟกต์ ให้เล่นก่อนวาร์ป
        if (teleportEffect != null) teleportEffect.Play();

        // ย้ายตำแหน่ง
        transform.position = nextPosition;

        Debug.Log("🔮 วงเวทย์วาร์ปไปที่จุด: " + spawnPoints[randomIndex].name);
    }
}