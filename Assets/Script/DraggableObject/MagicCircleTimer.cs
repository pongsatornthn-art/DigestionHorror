using UnityEngine;

public class MagicCircleTimer : MonoBehaviour
{
    [Header("การตั้งค่าเวลา")]
    public float stayDuration = 10f;
    private float timer;

    [Header("จุดสุ่มวงเวทย์")]
    public GameObject[] spawnPoints;

    public ParticleSystem teleportEffect;

    void Start()
    {
        // บรรทัดนี้สำคัญมาก! ถ้าโค้ดทำงาน มันต้องพิมพ์ข้อความนี้ออกมา
        Debug.Log("🔴 [ระบบรายงาน] เริ่มทำงานแล้ว! เจอวงเวทย์ในช่องทั้งหมด: " + spawnPoints.Length + " วง");

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("❌ [ระบบรายงาน] อ้าว! ช่อง Spawn Points ว่างเปล่า โค้ดเลยไม่ยอมทำอะไรต่อครับ!");
            return;
        }

        // ไล่ปิดวงเวทย์ทุกอัน
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                spawnPoints[i].SetActive(false);
                Debug.Log("🔴 [ระบบรายงาน] สั่งปิดวงเวทย์ที่ " + i + " สำเร็จ!");
            }
        }

        timer = stayDuration;
        SpawnRandomCircle();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnRandomCircle();
            timer = stayDuration;
        }
    }

    public void SpawnRandomCircle()
    {
        if (spawnPoints.Length == 0) return;

        // ปิดทั้งหมดก่อน
        foreach (GameObject circle in spawnPoints)
        {
            if (circle != null) circle.SetActive(false);
        }

        // สุ่มเปิด 1 อัน
        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject selectedCircle = spawnPoints[randomIndex];

        if (selectedCircle != null)
        {
            selectedCircle.SetActive(true);
            Debug.Log("🟢 [ระบบรายงาน] สุ่มเปิดวงเวทย์วงที่: " + randomIndex + " ชื่อ: " + selectedCircle.name);

            if (teleportEffect != null)
            {
                teleportEffect.transform.position = selectedCircle.transform.position;
                teleportEffect.Play();
            }
        }
    }
}