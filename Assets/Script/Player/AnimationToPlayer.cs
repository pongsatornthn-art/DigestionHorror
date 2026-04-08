using UnityEngine;

public class AnimationToPlayer : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        // คำสั่งนี้จะวิ่งไปหา PlayerController ที่แปะอยู่บนตัวแม่
        player = GetComponentInParent<PlayerController>();
    }

    // ⭐ ฟังก์ชันนี้แหละที่เราจะเอาไปใส่ใน Animation Event!
    public void TriggerDamageEvent()
    {
        if (player != null)
        {
            // สั่งให้ตัวแม่ทำการคำนวณดาเมจ
            player.DealDamage();
        }
    }
}