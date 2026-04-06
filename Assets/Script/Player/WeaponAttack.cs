using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [Header("Settings")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Body Parts (ลาก GameObject มาใส่ตรงนี้)")]
    public GameObject defaultBody; // ร่างปกติ
    public GameObject knifeBody;   // ร่างตอนถือมีด
    public GameObject axeBody;     // ร่างตอนถือขวาน (ID 1)
    public GameObject nailStickBody; // ร่างตอนถือไม้ตะปู (ID 2)

    private ItemData currentItem;

    [HideInInspector] public int bonusDamage = 0; // ⭐ เก็บดาเมจพิเศษจากวงเวทย์

    void Update()
    {
        currentItem = Inventory.instance.currentEquippedItem;
        UpdateBodyVisuals();

        if (Input.GetMouseButtonDown(0) && IsHoldingWeapon())
        {
            PerformAttack();
        }
    }

    bool IsHoldingWeapon()
    {
        return currentItem != null && currentItem.itemType == ItemType.Weapon;
    }

    void UpdateBodyVisuals()
    {
        if (currentItem == null || currentItem.itemType != ItemType.Weapon)
        {
            defaultBody.SetActive(true);
            knifeBody.SetActive(false);
            if (axeBody) axeBody.SetActive(false);
            if (nailStickBody) nailStickBody.SetActive(false);
            return;
        }

        // ปิดทุกร่างก่อน
        defaultBody.SetActive(false);
        knifeBody.SetActive(false);
        if (axeBody) axeBody.SetActive(false);
        if (nailStickBody) nailStickBody.SetActive(false);

        // เปิดเฉพาะร่างที่ถืออยู่
        switch (currentItem.weaponID)
        {
            case 0: knifeBody.SetActive(true); break;
            case 1: if (axeBody) axeBody.SetActive(true); break;
            case 2: if (nailStickBody) nailStickBody.SetActive(true); break;
            default: defaultBody.SetActive(true); break;
        }
    }

    void PerformAttack()
    {
        // สั่งเล่นแอนิเมชันตีตามร่างที่เปิดอยู่
        Animator currentAnim = null;
        if (knifeBody.activeSelf) currentAnim = knifeBody.GetComponent<Animator>();
        else if (axeBody != null && axeBody.activeSelf) currentAnim = axeBody.GetComponent<Animator>();
        else if (nailStickBody != null && nailStickBody.activeSelf) currentAnim = nailStickBody.GetComponent<Animator>();

        if (currentAnim != null)
        {
            currentAnim.SetTrigger("Attack");
        }

        // ⭐ คำนวณดาเมจรวม (ดาเมจอาวุธ + บัฟวงเวทย์)
        int finalDamage = currentItem.damage + bonusDamage;
        Debug.Log($"ฟัน! ความแรงอาวุธ {currentItem.damage} + บัฟ {bonusDamage} = โดนไป {finalDamage}");

        // โจมตีจริง
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;

                // ⭐ ส่งดาเมจที่บวกบัฟแล้วไปให้มอนสเตอร์
                stats.TakeDamage(finalDamage, currentItem.knockback, knockbackDir);

                // เช็คว่าทำให้เลือดไหลไหม
                if (currentItem.causesBleeding)
                {
                    stats.ApplyBleed(currentItem.bleedDuration, currentItem.bleedDamagePerSec);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}