using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public LayerMask enemyLayer; // ตั้ง Layer ของซอมบี้เป็น Enemy

    void Update()
    {
        // ถ้าคลิกซ้าย และถือของอยู่
        if (Input.GetMouseButtonDown(0) && Inventory.instance.currentEquippedItem != null)
        {
            Attack();
        }
    }

    void Attack()
    {
        // เล่น Animation ตี (ถ้ามี)
        // Animator.SetTrigger("Attack"); 

        // เช็คว่าตีโดนใครบ้าง
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("ตีโดน: " + enemy.name);
            Destroy(enemy.gameObject); // ฆ่าซอมบี้ทิ้ง (หรือลดเลือด)
        }
    }

    // วาดวงกลมระยะตีให้เห็นในหน้า Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}