using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [Header("Settings")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Body Parts (ลาก GameObject มาใส่ตรงนี้)")]
    public GameObject defaultBody; // เช่น Body_Pivot (ร่างปกติ)
    public GameObject knifeBody;   // ร่างตอนถือมีด
    // public GameObject axeBody;  // ร่างตอนถือขวาน (เอาไว้ใส่ทีหลัง)

    private ItemData currentItem; // เก็บของที่ถืออยู่ปัจจุบัน

    void Update()
    {
        // 1. ดึงข้อมูลจากกระเป๋ามาเช็คตลอดเวลา
        currentItem = Inventory.instance.currentEquippedItem;

        // 2. เรียกฟังก์ชันจัดการเปลี่ยนร่าง
        UpdateBodyVisuals();

        // 3. ถ้าคลิกซ้าย + ถือมีดอยู่ -> สั่งตี
        if (Input.GetMouseButtonDown(0) && IsHoldingWeapon())
        {
            PerformAttack();
        }
    }

    // ฟังก์ชันเช็คว่าถืออาวุธอยู่ไหม (เขียนแยกออกมาจะได้ดูง่ายๆ)
    bool IsHoldingWeapon()
    {
        return currentItem != null && currentItem.itemType == ItemType.Weapon;
    }

    // ฟังก์ชันเปลี่ยนร่าง (หัวใจสำคัญที่คุณต้องการ)
    void UpdateBodyVisuals()
    {
        // --- กรณีที่ 1: ไม่ได้ถืออะไรเลย หรือ ถือของที่ไม่ใช่อาวุธ ---
        if (currentItem == null || currentItem.itemType != ItemType.Weapon)
        {
            defaultBody.SetActive(true);  // เปิดร่างปกติ
            knifeBody.SetActive(false);   // ปิดร่างมีด
            return;
        }

        // --- กรณีที่ 2: ถืออาวุธ (เช็คตาม ID) ---
        if (currentItem.itemType == ItemType.Weapon)
        {
            switch (currentItem.weaponID)
            {
                case 0: // ID 0 = มีด
                    defaultBody.SetActive(false); // ปิดร่างปกติ
                    knifeBody.SetActive(true);    // เปิดร่างมีด
                    break;

                // case 1: // ID 1 = ขวาน (อนาคตค่อยมาเติม)
                //     defaultBody.SetActive(false);
                //     knifeBody.SetActive(false);
                //     axeBody.SetActive(true);
                //     break;

                default: // กันพลาด ถ้า ID ไม่ตรงให้กลับไปร่างปกติ
                    defaultBody.SetActive(true);
                    knifeBody.SetActive(false);
                    break;
            }
        }
    }

    void PerformAttack()
    {
        // ตรงนี้ใส่ Animator ของร่างมีด หรือร่างปัจจุบันที่เปิดอยู่
        // เนื่องจากเราสลับ GameObject คุณอาจจะต้อง Getcomponent Animator จากตัวที่เปิดอยู่
        Animator currentAnim = knifeBody.GetComponent<Animator>();

        if (currentAnim != null)
        {
            currentAnim.SetTrigger("Attack"); // สั่งเล่นท่าตีในร่างมีด
        }

        // คำนวณ Damage
        Debug.Log($"ฟันด้วยมีด! ความแรง {currentItem.damage}");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("โดน: " + enemy.name);
            // Destroy(enemy.gameObject); 
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}