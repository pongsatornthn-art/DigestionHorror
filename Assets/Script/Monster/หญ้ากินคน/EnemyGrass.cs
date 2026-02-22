using UnityEngine;
using System.Collections;

public class EnemyGrass : MonoBehaviour
{
    [Header("Settings")]
    public float trapDuration = 2f;      // ระยะเวลาที่โดนจับ (วินาที)
    public float damagePerSecond = 2f;   // ดาเมจ HP ต่อวินาที
    public float digestionPerSecond = 5f; // เพิ่มค่า Digestion ต่อวินาที
    public float cooldown = 3f;          // ระยะเวลาคูลดาวน์ก่อนจะจับได้ใหม่

    private bool isCooldown = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCooldown)
        {
            StartCoroutine(TrapPlayer(other.gameObject));
        }
    }

    IEnumerator TrapPlayer(GameObject player)
    {
        isCooldown = true;
        PlayerStatus status = player.GetComponent<PlayerStatus>();

        if (status != null)
        {
            status.isRooted = true;
            Debug.Log("Player ถูกหญ้าจับไว้!");

            float elapsed = 0f;
            float damageAccumulator = 0f; // ตัวสะสมดาเมจ

            while (elapsed < trapDuration)
            {
                // 1. เพิ่มค่า Digestion (ทำงานปกติ)
                if (DigestionSystem.instance != null)
                    DigestionSystem.instance.IncreaseDigestion(digestionPerSecond * Time.deltaTime);

                // 2. ปรับปรุงการทำดาเมจ HP แบบค่อยๆ ลด
                if (PlayerController.instance != null)
                {
                    // สะสมดาเมจไว้ในตัวแปร float
                    damageAccumulator += damagePerSecond * Time.deltaTime;

                    // เมื่อสะสมครบ 1 หน่วย หรือมากกว่า ให้ทำการหัก HP
                    if (damageAccumulator >= 1f)
                    {
                        int damageToDeal = Mathf.FloorToInt(damageAccumulator);
                        PlayerController.instance.PlayerTakeDamage(damageToDeal);
                        damageAccumulator -= damageToDeal; // หักส่วนที่ลดไปแล้วออก เหลือเศษไว้สะสมต่อ
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            status.isRooted = false;
            Debug.Log("Player หลุดจากการโดนจับ");
        }

        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }
}