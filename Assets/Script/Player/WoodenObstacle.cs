using UnityEngine;

public class WoodenObstacle : MonoBehaviour
{
    [Header("ตั้งค่าไม้ขวางทาง")]
    public int hp = 30;

    [Tooltip("พิมพ์ชื่ออาวุธที่ใช้พังได้ให้ตรงกับ itemName ใน ItemData")]
    public string requiredWeaponName = "Axe";

    public GameObject breakEffect; // ใส่พาร์ทิเคิลไม้กระจายตอนพังได้ (ถ้ามี)

    // ฟังก์ชันรับดาเมจ (ถูกเรียกจาก PlayerController)
    public void HitObstacle(int damage, ItemData weaponUsed)
    {
        // 1. เช็คว่าถืออาวุธอยู่ไหม และชื่ออาวุธตรงกับที่ต้องการไหม
        if (weaponUsed == null || weaponUsed.itemName != requiredWeaponName)
        {
            Debug.Log("❌ ฟันไม่เข้า! ต้องใช้ " + requiredWeaponName + " เท่านั้น!");
            // เคล็ดลับ: ตรงนี้เอา AudioSource มาเล่นเสียง "ตึ้ง" (เสียงฟันของแข็ง) ได้ครับ
            return;
        }

        // 2. ถ้าใช้อาวุธถูกประเภท ให้ลดเลือดไม้
        hp -= damage;
        Debug.Log("🪓 ไม้โดนฟัน! เลือดไม้เหลือ: " + hp);

        // 3. เลือดหมด = พัง
        if (hp <= 0)
        {
            BreakObstacle();
        }
    }

    void BreakObstacle()
    {
        Debug.Log("💥 ทางเปิดแล้ว!");
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject); // ลบไม้ทิ้ง เปิดทางให้เดินผ่าน
    }
}