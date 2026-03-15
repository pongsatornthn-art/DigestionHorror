using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
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
    public GameObject pauseMainPanel;
    public GameObject savePanel;
    public GameObject loadPanel;
    public GameObject settingsPanel;

    [Header("Audio Settings (ตั้งค่าเสียง)")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Save System (Hardcore Limit)")]
    public int maxSavesAllowed = 3;
    public int currentSaveCount = 0;

    private bool isPaused = false;
    private bool isAudioInitialized = false;

    void Awake()
    {
        instance = this;
        currentSaveCount = PlayerPrefs.GetInt("SaveCount", 0);
    }

    void Start()
    {
        // 1. โหลดตั้งค่ามาใส่หลอด Slider ทิ้งไว้
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

        // 2. ปิดหน้าต่างทั้งหมดตอนเริ่มเกม
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // ⭐ [ไม้ตายก้นหีบ] ตื้อ Mixer จนกว่าจะยอมตื่น!
        if (!isAudioInitialized && mainMixer != null)
        {
            // 1. ดึงค่าเซฟ
            float savedMusic = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
            float savedSFX = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

            // 2. เซฟตี้กันบั๊กเสียงหาย (ถ้าเป็น 0 ให้เด้งกลับมา 75%)
            if (savedMusic <= 0.001f) savedMusic = 0.75f;
            if (savedSFX <= 0.001f) savedSFX = 0.75f;

            // 3. แปลงค่าเป็นเดซิเบลเพื่อเตรียมยิง
            float dbMusic = Mathf.Log10(savedMusic) * 20f;
            float dbSFX = Mathf.Log10(savedSFX) * 20f;

            // 4. 🎯 ลองยิงคำสั่ง! ถ้า Mixer โหลดเสร็จและรับคำสั่ง มันจะคืนค่าเป็น true
            bool isMixerReady = mainMixer.SetFloat("MusicVol", dbMusic);

            if (isMixerReady)
            {
                // ถ้ายอมรับค่า Music แล้ว ก็ยิงค่า SFX ตามไปเลย
                mainMixer.SetFloat("SFXVol", dbSFX);

                // อัปเดตสไลเดอร์ให้ตรงกัน
                if (musicSlider != null) musicSlider.value = savedMusic;
                if (sfxSlider != null) sfxSlider.value = savedSFX;

                isAudioInitialized = true; // ล็อกกุญแจ! ไม่ต้องทำซ้ำแล้ว
                Debug.Log("🔊 Audio Mixer ตื่นเต็มตาและรับคำสั่งแล้ว!");
            }
        }

        // ระบบกดปุ่ม Esc เปิดหน้าต่างของคุณ (โค้ดเดิม)
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
        ShowPauseMainMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

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
            mainMixer.SetFloat("MusicVol", dbValue);
            PlayerPrefs.SetFloat("SavedMusicVol", sliderValue);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (mainMixer != null)
        {
            float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            mainMixer.SetFloat("SFXVol", dbValue);
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
            ResumeGame();
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