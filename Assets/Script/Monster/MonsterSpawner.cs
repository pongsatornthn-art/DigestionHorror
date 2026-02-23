using UnityEngine;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Types")]
    public GameObject[] monsterPrefabs;

    [Header("Settings")]
    public float spawnInterval = 10f;
    public int baseSpawnAmount = 1;

    [Header("Spawn Area Settings")]
    public float spawnRadius = 5f;

    // ⭐ 1. เพิ่มตัวแปรสำหรับตรวจจับ Safe Zone
    [Header("Safe Zone Protection")]
    public LayerMask safeZoneLayer; // กำหนด Layer ของ SafeZone
    public float monsterCheckRadius = 0.5f; // รัศมีตัวมอนสเตอร์เอาไว้เช็คชน (ปรับตามขนาดมอนสเตอร์)

    private float timer;
    private List<GameObject> spawnedMonsters = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMonsters();
            timer = 0;
        }

        CheckAndRemoveExtraMonsters();
    }

    void SpawnMonsters()
    {
        if (monsterPrefabs.Length == 0) return;

        int bonus = 0;
        if (DigestionSystem.instance != null)
        {
            bonus = DigestionSystem.instance.GetSpawnBonus();
        }

        int totalAllowed = Mathf.Min(baseSpawnAmount + bonus, 10);

        while (spawnedMonsters.Count < totalAllowed)
        {
            int randomIndex = Random.Range(0, monsterPrefabs.Length);

            // ⭐ 2. เพิ่มระบบสุ่มหาจุดเกิดที่ "ไม่ทับ" Safe Zone
            Vector3 finalSpawnPos = Vector3.zero;
            bool isValidPosition = false;
            int maxAttempts = 10; // สุ่มหาจุดสูงสุด 10 ครั้ง (กันลูปค้างถ้าตั้ง Spawner กลางบ้าน)

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 checkPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

                // ตรวจสอบว่าจุดเช็คนี้ ไปชนกับ Layer SafeZone หรือไม่
                Collider2D hit = Physics2D.OverlapCircle(checkPos, monsterCheckRadius, safeZoneLayer);

                // ถ้าไม่ชนอะไรเลย (hit == null) แปลว่าจุดนี้ปลอดภัย!
                if (hit == null)
                {
                    finalSpawnPos = checkPos;
                    isValidPosition = true;
                    break; // หยุดสุ่ม
                }
            }

            // ถ้าสุ่มได้ตำแหน่งที่ถูกต้อง ค่อยเสกมอนสเตอร์
            if (isValidPosition)
            {
                GameObject newMonster = Instantiate(monsterPrefabs[randomIndex], finalSpawnPos, Quaternion.identity);
                spawnedMonsters.Add(newMonster);
            }
            else
            {
                // ถ้าสุ่ม 10 ครั้งแล้วยังลง Safe Zone ตลอด ให้ข้ามการเกิดตัวนี้ไปก่อน
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    void CheckAndRemoveExtraMonsters()
    {
        int bonus = 0;
        if (DigestionSystem.instance != null)
        {
            bonus = DigestionSystem.instance.GetSpawnBonus();
        }
        int totalAllowed = Mathf.Min(baseSpawnAmount + bonus, 10);

        spawnedMonsters.RemoveAll(item => item == null);

        if (spawnedMonsters.Count > totalAllowed)
        {
            int amountToRemove = spawnedMonsters.Count - totalAllowed;
            for (int i = 0; i < amountToRemove; i++)
            {
                if (spawnedMonsters.Count > 0)
                {
                    GameObject target = spawnedMonsters[0];
                    spawnedMonsters.RemoveAt(0);
                    Destroy(target);
                }
            }
        }
    }
}