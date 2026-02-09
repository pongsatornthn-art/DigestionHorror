using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;
    public int damageToPlayer = 10;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log(name + " เลือดเหลือ " + hp);
        if (hp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // เรียกใช้ฟังก์ชันที่เราเพิ่มใน PlayerController แล้ว
                player.PlayerTakeDamage(damageToPlayer);
            }
        }
    }
}