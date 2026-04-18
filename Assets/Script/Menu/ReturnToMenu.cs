using UnityEngine;
using UnityEngine.Video; // จำเป็นต้องใช้เพื่อคุม Video Player
using UnityEngine.SceneManagement; // จำเป็นต้องใช้เพื่อเปลี่ยนซีน

public class ReturnToMenu : MonoBehaviour
{
    [Header("ชื่อซีนหน้าเมนู (ต้องพิมพ์ให้ตรงเป๊ะ)")]
    public string menuSceneName = "Menu";

    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // เมื่อวิดีโอเล่นจบ (ถึงจุด Loop Point) ให้เรียกฟังก์ชัน GoToMenu
        videoPlayer.loopPointReached += GoToMenu;
    }

    void GoToMenu(VideoPlayer vp)
    {
        // คำสั่งเปลี่ยนไปซีนหน้าเมนู
        SceneManager.LoadScene(menuSceneName);
    }

    // แถม: เผื่อผู้เล่นอยากกดข้ามวิดีโอด้วยปุ่ม Spacebar
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}