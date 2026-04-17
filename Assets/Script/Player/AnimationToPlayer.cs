using UnityEngine;

public class AnimationToPlayer : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        // คำสั่งนี้จะวิ่งไปหา PlayerController ที่แปะอยู่บนตัวแม่
        player = GetComponentInParent<PlayerController>();
    }

    // * ฟังก์ชันนี้แหละที่เราจะเอาไปใส่ใน Animation Event!
    // รับค่าตัวเลขจากหน้า Animation (1 = ฟันเบา, 0 = ฟันหนัก)
    public void TriggerDamageEvent(int isLightAttack)
    {
        if (player != null)
        {
            // สั่งให้ตัวแม่ทำการคำนวณดาเมจ
            bool isLight = (isLightAttack == 1);
            player.DealDamageWithAttackType(isLight);
        }
    }
}