using UnityEngine;

public class EnemyGrass : MonoBehaviour
{
    [Header("ความรุนแรง")]
    public float digestionDamage = 5f; // เพิ่มค่า Digestion วิละเท่าไหร่

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // เรียกใช้ระบบ Digestion (เพิ่มค่าการย่อยสลาย)
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(digestionDamage * Time.deltaTime);
            }
        }
    }
}