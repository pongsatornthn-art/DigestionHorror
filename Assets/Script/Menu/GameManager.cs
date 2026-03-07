using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // ⭐ ต้องเพิ่มอันนี้สำหรับ AudioMixer
using System.IO;

[System.Serializable]
public class SaveData
{
    public float playerPosX, playerPosY;
    public int health;
    public float stamina;
    public int money;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Panels (หน้าต่างต่างๆ)")]
    public GameObject pauseMainPanel; // หน้าหยุดเกมหลัก (มีปุ่ม Resume, Save, Load, Settings)
    public GameObject savePanel;      // หน้าเลือกช่องเซฟ
    public GameObject loadPanel;      // หน้าเลือกช่องโหลด
    public GameObject settingsPanel;  // หน้าตั้งค่าเสียง

    [Header("Audio Settings (ตั้งค่าเสียง)")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Save System (Hardcore Limit)")]
    public int maxSavesAllowed = 3;
    public int currentSaveCount = 0;

    private bool isPaused = false;

    void Awake()
    {
        instance = this;
        currentSaveCount = PlayerPrefs.GetInt("SaveCount", 0);
    }

    void Start()
    {
        // โหลดตั้งค่าเสียงที่เคยเซฟไว้ (ค่าเริ่มต้นคือ 0.75)
        float savedMusic = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        // ปิดหน้าต่างทั้งหมดตอนเริ่มเกม
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ==========================================
    // 🛑 ระบบหยุดเกมและสลับหน้าต่าง UI
    // ==========================================
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        ShowPauseMainMenu(); // เปิดมาให้เจอหน้าหลักก่อนเสมอ
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // ปิดทุกหน้าต่าง
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // ฟังก์ชันสำหรับผูกกับปุ่มกด เพื่อสลับหน้าต่างไปมา
    public void ShowPauseMainMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(true);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void ShowSaveMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(true);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void ShowLoadMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    // ==========================================
    // 🔊 ระบบตั้งค่าเสียง (ปรับปุ๊บ เซฟปั๊บ)
    // ==========================================
    public void SetMusicVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            mainMixer.SetFloat("MusicVol", dbValue); // ชื่ออ้างอิงจาก Audio Mixer
            PlayerPrefs.SetFloat("SavedMusicVol", sliderValue);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            mainMixer.SetFloat("SFXVol", dbValue); // ชื่ออ้างอิงจาก Audio Mixer
            PlayerPrefs.SetFloat("SavedSFXVol", sliderValue);
        }
    }

    // ==========================================
    // 💾 ระบบเซฟและโหลด (แบบฮาร์ดคอร์)
    // ==========================================
    public void SaveGame(int slotNumber)
    {
        if (currentSaveCount >= maxSavesAllowed)
        {
            Debug.Log("❌ เซฟไม่ได้แล้ว! โควต้าครบ 3 ครั้งแล้ว");
            return;
        }

        SaveData data = new SaveData();

        if (PlayerController.instance != null)
        {
            data.playerPosX = PlayerController.instance.transform.position.x;
            data.playerPosY = PlayerController.instance.transform.position.y;
            data.health = PlayerController.instance.currentHealth;
            data.stamina = PlayerController.instance.currentStamina;
            data.money = PlayerController.instance.currentMoney;
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveSlot_" + slotNumber, json);

        currentSaveCount++;
        PlayerPrefs.SetInt("SaveCount", currentSaveCount);
        PlayerPrefs.Save();

        Debug.Log($"✅ เซฟเกมลงช่อง {slotNumber} สำเร็จ! เหลือสิทธิ์เซฟอีก {maxSavesAllowed - currentSaveCount} ครั้ง");

        // ⭐ สำคัญ: เมื่อเซฟเสร็จ ให้สั่งเปลี่ยนไปหน้าต่าง Load ทันที
        ShowLoadMenu();
    }

    public void LoadGame(int slotNumber)
    {
        if (PlayerPrefs.HasKey("SaveSlot_" + slotNumber))
        {
            string json = PlayerPrefs.GetString("SaveSlot_" + slotNumber);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (PlayerController.instance != null)
            {
                PlayerController.instance.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                PlayerController.instance.transform.position = new Vector2(data.playerPosX, data.playerPosY);

                PlayerController.instance.currentHealth = data.health;
                PlayerController.instance.currentStamina = data.stamina;
                PlayerController.instance.currentMoney = data.money;

                PlayerController.instance.UpdateUI();
            }

            Debug.Log($"📂 โหลดเกมจากช่อง {slotNumber} สำเร็จ!");
            ResumeGame(); // โหลดเสร็จ ปิดหน้าต่างเล่นต่อเลย
        }
        else
        {
            Debug.Log("❌ ไม่มีข้อมูลเซฟในช่องนี้!");
        }
    }

    public void ResetSaveQuota()
    {
        currentSaveCount = 0;
        PlayerPrefs.SetInt("SaveCount", 0);
        PlayerPrefs.Save();
    }
}