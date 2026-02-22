using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 10; // เปลี่ยนเป็น int เพื่อให้ตรงกับ PlayerTakeDamage
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // 1. ลดเลือดผู้เล่นผ่านระบบหลัก
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerTakeDamage(damage);
                Debug.Log("หินกระแทกผู้เล่น! ลด HP: " + damage);
            }

            // 2. ระบบ Digestion (ถ้ายังมีอยู่)
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion((float)damage);
            }

            Destroy(gameObject);
        }
        else if (!other.gameObject.CompareTag("Enemy"))
        {
            // ชนกำแพงหรือสิ่งกีดขวางอื่นๆ
            Destroy(gameObject);
        }
    }
}