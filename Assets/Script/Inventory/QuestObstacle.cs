using UnityEngine;
using System.Collections;

public class QuestObstacle : MonoBehaviour
{
    public ItemData requiredTool;   // ต้องใช้ "FinalAxe"
    public float maxHP = 50f;       // เลือดต้นไม้
    public GameObject enemyPrefab;  // ตัวซอมบี้
    public Transform[] spawnPoints; // จุดเกิดซอมบี้

    private float currentHP;
    private bool eventStarted = false;

    void Start() { currentHP = maxHP; }

    void OnMouseOver()
    {
        // กดคลิกซ้ายเพื่อฟัน
        if (Input.GetMouseButtonDown(0))
        {
            Hit();
        }
    }

    void Hit()
    {
        if (!Inventory.instance.HasItem(requiredTool))
        {
            Debug.Log("ไม่มีขวาน!");
            return;
        }

        if (!eventStarted)
        {
            eventStarted = true;
            StartCoroutine(SpawnZombies());
        }

        currentHP -= 10;
        // เขย่าต้นไม้เล็กน้อย
        transform.position += (Vector3)Random.insideUnitCircle * 0.1f;

        if (currentHP <= 0)
        {
            Destroy(gameObject); // ทางเปิด
            StopAllCoroutines();
        }
    }

    IEnumerator SpawnZombies()
    {
        while (currentHP > 0)
        {
            if (spawnPoints.Length > 0)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Instantiate(enemyPrefab, sp.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(3f); // เกิดทุก 3 วิ
        }
    }
}