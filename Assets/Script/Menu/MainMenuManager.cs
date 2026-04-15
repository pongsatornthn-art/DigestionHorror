using UnityEngine;
using UnityEngine.SceneManagement; // ต้องใช้ตัวนี้สำหรับการเปลี่ยนหน้าซีน

public class MainMenuManager : MonoBehaviour
{
    [Header("ตั้งค่าหน้าต่าง UI (ลาก Panel มาใส่)")]
    public GameObject mainMenuPanel; // หน้าต่างเมนูหลักที่มีปุ่ม Start, Quit
    public GameObject settingsPanel; // หน้าต่างตั้งค่า (เอาไว้ซ่อน/โชว์)

    [Header("ตั้งค่าการโหลดซีน")]
    [Tooltip("พิมพ์ชื่อ Scene เกมของคุณให้เป๊ะๆ (ตัวพิมพ์เล็ก/ใหญ่ต้องตรงกัน)")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        // เริ่มเกมมา ให้โชว์หน้าเมนูหลัก และซ่อนหน้าตั้งค่าเอาไว้ก่อน
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ฟังก์ชันสำหรับปุ่ม "เริ่มเกม"
    public void PlayGame()
    {
        Debug.Log("🎮 กำลังโหลดเข้าฉากเกม...");
        SceneManager.LoadScene(gameSceneName);
    }

    // ฟังก์ชันสำหรับปุ่ม "ตั้งค่า" (กดแล้วสลับหน้าต่าง)
    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // ฟังก์ชันสำหรับปุ่ม "ย้อนกลับ" ในหน้าตั้งค่า
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // ฟังก์ชันสำหรับปุ่ม "ออกจากเกม"
    public void QuitGame()
    {
        Debug.Log("🚪 ปิดเกมแล้วจ้า!");

        // คำสั่งนี้จะทำงานตอนที่พอร์ตเกมเป็นไฟล์ .exe หรือลงมือถือแล้วเท่านั้น
        Application.Quit();
    }
    // ฟังก์ชันสำหรับรับค่าจากหลอด Slider มาปรับระดับเสียงเกม
    public void SetGlobalVolume(float sliderValue)
    {
        // ปรับระดับเสียงรวมของทั้งเกม (0.0 คือเงียบสุด, 1.0 คือดังสุด)
        AudioListener.volume = sliderValue;
    }
}