using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MagicCircle : MonoBehaviour
{
    [Header("ตั้งค่าวงเวทย์")]
    public string requiredTag = "MagicPuppet"; // แท็กของหุ่นที่เราต้องลากมาวาง
    public int buffDamageAmount = 20;          // จำนวนดาเมจที่จะบวกเพิ่มให้ผู้เล่น

    [Header("ภาพวงเวทย์ (สลับตอนทำงาน)")]
    public GameObject inactiveGraphic; // รูปวงเวทย์ตอนยังไม่ติด (สีเทา)
    public GameObject activeGraphic;   // รูปวงเวทย์ตอนติดแล้ว (สีแดงสว่าง)

    private WeaponAttack playerWeapon;

    void Start()
    {
        // เริ่มเกมมาให้วงเวทย์ปิดอยู่
        UpdateVisuals(false);

        // แอบหาตัวสคริปต์โจมตีของผู้เล่นมารอไว้เลย
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerWeapon = player.GetComponentInChildren<WeaponAttack>();
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติ "เมื่อมีอะไรบางอย่างเข้ามาชน/ทับ"
    void OnTriggerEnter2D(Collider2D other)
    {
        // เช็คว่าสิ่งที่ลากมาทับ มีแท็กตรงกับที่เราตั้งไว้ไหม (ป้องกันเอาตู้ธรรมดามาวางแล้วติดบัฟ)
        if (other.CompareTag(requiredTag))
        {
            Debug.Log("✅ หุ่นเข้าวงเวทย์แล้ว! ผู้เล่นได้รับบัฟโจมตี!");
            UpdateVisuals(true);

            // ส่งบัฟดาเมจให้ผู้เล่น
            if (playerWeapon != null)
            {
                playerWeapon.bonusDamage += buffDamageAmount;
            }
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติ "เมื่อสิ่งนั้นถูกลากออกไป"
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(requiredTag))
        {
            Debug.Log("❌ หุ่นหลุดออกจากวงเวทย์! พลังโจมตีกลับเป็นปกติ");
            UpdateVisuals(false);

            // ดึงบัฟดาเมจกลับคืน
            if (playerWeapon != null)
            {
                playerWeapon.bonusDamage -= buffDamageAmount;
            }
        }
    }

    // ฟังก์ชันสลับรูปภาพวงเวทย์
    void UpdateVisuals(bool isActive)
    {
        if (inactiveGraphic != null) inactiveGraphic.SetActive(!isActive);
        if (activeGraphic != null) activeGraphic.SetActive(isActive);
    }
}