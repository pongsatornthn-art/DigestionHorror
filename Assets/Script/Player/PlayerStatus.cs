using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public bool isRooted = false; // สถานะโดนจับ

    // ฟังก์ชันสำหรับโดนดาเมจทั่วไป (ถ้าต้องการแยกจาก Digestion)
    public void TakeDamage(int damage)
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.PlayerTakeDamage(damage);
        }
    }
}