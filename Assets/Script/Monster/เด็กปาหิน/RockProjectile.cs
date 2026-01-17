using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;
    public float lifeTime = 3f; // ⭐ หินจะหายไปเองภายใน 3 วินาทีถ้าไม่ชนอะไร

    void Start()
    {
        // สั่งทำลายตัวเองล่วงหน้าตามเวลาที่ตั้งไว้
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // 1. ถ้าชนผู้เล่น (Player)
        if (other.gameObject.CompareTag("Player"))
        {
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(damage);
                Debug.Log("โดนหินปาเข้าเต็มๆ!");
            }
            Destroy(gameObject); // ชนแล้วหายไปทันที
        }
        // 2. ถ้าชนมอนสเตอร์ (Enemy)
        else if (other.gameObject.CompareTag("Enemy"))
        {
            // ไม่ต้องทำอะไร ปล่อยให้หินบินผ่านไป (กันหินระเบิดคามือเด็ก)
            // เราไม่สั่ง Destroy ตรงนี้เพื่อให้มันบินทะลุพวกเดียวกันได้
        }
        // 3. ถ้าชนอย่างอื่น (เช่น กำแพง)
        else
        {
            Destroy(gameObject); // ชนกำแพงแล้วแตกทันที
        }
    }
}