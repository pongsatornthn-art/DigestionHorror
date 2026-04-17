using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("หน้าต่าง UI")]
    public GameObject mainMenuPanel;
    public GameObject loadPanel;
    public GameObject settingsPanel;

    [Header("การตั้งค่า Scene")]
    public string newGameSceneName = "Newgame"; // 👈 ใส่ชื่อซีนสำหรับปุ่ม New Game
    public string mainGameSceneName = "Main";   // 👈 ใส่ชื่อซีนสำหรับปุ่ม Start Game

    [Header("ระบบโหลดเซฟ UI")]
    public TMP_Text[] slotTexts;

    [Header("ระบบเสียง")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        ShowMainMenu();
        UpdateLoadUI();

        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);
    }

    // ==========================================
    // 🚀 ระบบปุ่มเข้าเกม (แยก 2 ปุ่ม)
    // ==========================================

    // ⭐ ฟังก์ชันสำหรับปุ่ม "New Game" -> ไปฉาก Newgame
    public void NewGame()
    {
        Debug.Log("🌱 โหลดหน้า Newgame...");
        PlayerPrefs.SetInt("SlotToLoad", 0);
        SceneManager.LoadScene(newGameSceneName);
    }

    // ⭐ ฟังก์ชันสำหรับปุ่ม "Start Game" -> ดิ่งไปฉาก Main เลย
    public void StartGame()
    {
        Debug.Log("⚔️ เข้าเกมหลัก (Main) ทันที...");
        PlayerPrefs.SetInt("SlotToLoad", 0);
        SceneManager.LoadScene(mainGameSceneName);
    }

    public void LoadGameFromMenu(int slotNumber)
    {
        if (PlayerPrefs.HasKey("SaveSlot_" + slotNumber))
        {
            Debug.Log($"💾 กำลังโหลดช่องที่ {slotNumber} เข้าฉากหลัก...");
            PlayerPrefs.SetInt("SlotToLoad", slotNumber);
            SceneManager.LoadScene(mainGameSceneName);
        }
        else
        {
            Debug.Log("❌ ช่องนี้ไม่มีเซฟครับ!");
        }
    }

    // ==========================================
    // 🖥️ ระบบสลับหน้าต่าง และ อัปเดตข้อความเซฟ
    // ==========================================
    public void ShowMainMenu()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void ShowLoadMenu()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        UpdateLoadUI();
    }

    public void ShowSettingsMenu()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void UpdateLoadUI()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            int slotNumber = i + 1;
            if (slotTexts[i] == null) continue;

            if (PlayerPrefs.HasKey("SaveSlot_" + slotNumber))
            {
                slotTexts[i].text = "ช่องเซฟ " + slotNumber + " : มีข้อมูล 💾";
                slotTexts[i].color = new Color(0.2f, 0.8f, 0.2f);
            }
            else
            {
                slotTexts[i].text = "ช่องเซฟ " + slotNumber + " : ว่างเปล่า";
                slotTexts[i].color = Color.white;
            }
        }
    }

    // ==========================================
    // 🔊 ระบบตั้งค่าเสียง
    // ==========================================
    public void SetMusicVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f);
            PlayerPrefs.SetFloat("SavedMusicVol", sliderValue);
        }
    }
    public void SetSFXVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f);
            PlayerPrefs.SetFloat("SavedSFXVol", sliderValue);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}