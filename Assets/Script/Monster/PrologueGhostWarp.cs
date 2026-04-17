using UnityEngine;
using UnityEngine.SceneManagement;

public class PrologueGhostWarp : MonoBehaviour
{
    [Header("ฉากที่จะวาร์ปไป")]
    public string targetScene = "Main";

    // 1. กรณีที่ Collider เป็นแบบชนกันปกติ (ไม่ติ๊ก Is Trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            WarpToMainGame();
        }
    }

    // 2. กรณีที่ Collider เป็นแบบเดินทะลุได้ (ติ๊ก Is Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WarpToMainGame();
        }
    }

    // ฟังก์ชันสั่งวาร์ป
    private void WarpToMainGame()
    {
        Debug.Log("👻 ผู้เล่นโดนผีแตะตัว! วาร์ปเข้าเกมหลัก...");

        // ฝากเลข 0 เพื่อบอกระบบว่าเป็นการเริ่มเกมใหม่ (ไม่โหลดเซฟ)
        PlayerPrefs.SetInt("SlotToLoad", 0);

        // โหลดเข้าฉากเกมหลักทันที
        SceneManager.LoadScene(targetScene);
    }
}