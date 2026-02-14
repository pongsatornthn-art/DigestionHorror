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
    // ✅ เพิ่มตัวแปรนี้เพื่อกำหนดความกว้างในการกระจายตัว
    public float spawnRadius = 5f;

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

            // ✅ เปลี่ยนวิธีคำนวณจุดเกิดให้กระจายเป็นวงกลมรอบจุดสปาวน์
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

            GameObject newMonster = Instantiate(monsterPrefabs[randomIndex], spawnPos, Quaternion.identity);
            spawnedMonsters.Add(newMonster);
        }
    }

    // ฟังก์ชันช่วยวาดขอบเขตในหน้า Scene (เส้นสีเขียว) เพื่อให้คุณกะระยะได้ง่ายขึ้น
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