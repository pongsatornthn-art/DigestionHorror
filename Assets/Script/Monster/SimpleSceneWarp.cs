using UnityEngine;
using UnityEngine.SceneManagement; // ต้องมีบรรทัดนี้เพื่อใช้คำสั่งเปลี่ยนฉาก

public class SimpleSceneWarp : MonoBehaviour
{
    [Header("ชื่อฉากที่ต้องการจะข้ามไป")]
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // เช็คว่าคนที่เดินมาชนคือ Player ใช่ไหม
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}