using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int hp = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log(name + " โดนตี! เลือดเหลือ " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // ตายแล้วลบตัวเองทิ้ง
            // เพิ่ม Effect เสียง/ระเบิด ตรงนี้ได้
        }
    }
}