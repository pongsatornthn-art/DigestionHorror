using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("ตั้งค่าเงิน")]
    public int minMoney = 10; // สุ่มเงินขั้นต่ำ
    public int maxMoney = 50; // สุ่มเงินสูงสุดสุด

    void OnTriggerEnter2D(Collider2D other)
    {
        // เช็คว่าคนที่มาชนคือ Player หรือเปล่า?
        if (other.CompareTag("Player"))
        {
            // 1. สุ่มจำนวนเงินที่จะได้
            int moneyToGive = Random.Range(minMoney, maxMoney + 1);

            // 2. เอาเงินไปบวกใส่กระเป๋าผู้เล่น
            PlayerController.instance.currentMoney += moneyToGive;

            // 3. สั่งให้อัปเดตตัวเลขบนหน้าจอ
            PlayerController.instance.SendMessage("UpdateUI");

            Debug.Log($"เก็บเงินได้! +{moneyToGive} บาท (ยอดรวม: {PlayerController.instance.currentMoney})");

            // 4. ลบเหรียญทิ้งออกจากฉาก
            Destroy(gameObject);
        }
    }
}