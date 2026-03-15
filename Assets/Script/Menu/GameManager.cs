using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.IO;
using TMPro; // ⭐ เพิ่มไลบรารีนี้สำหรับจัดการข้อความ TextMeshPro

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

    [Header("Save UI Texts (ข้อความสถานะเซฟ)")] // ⭐ เพิ่มส่วนนี้สำหรับลาก Text มาใส่
    public TMP_Text[] slotTexts; // ช่องใส่ Text ของ Slot 1, 2, 3
    public TMP_Text quotaText;   // ช่องใส่ Text บอกโควต้าการเซฟที่เหลือ

    private bool isPaused = false;
    private bool isAudioInitialized = false; // ⭐ ระบบป้องกันเสียงบั๊ก

    void Awake()
    {
        instance = this;
        currentSaveCount = PlayerPrefs.GetInt("SaveCount", 0);
    }

    void Start()
    {
        // โหลดตั้งค่าเสียงที่เคยเซฟไว้
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

        // ปิดหน้าต่างทั้งหมดตอนเริ่มเกม
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        // ⭐ อัปเดตข้อความเซฟตั้งแต่เริ่มเกม
        UpdateSaveUI();
    }

    void Update()
    {
        // ⭐ [ระบบแก้บั๊กเสียง] ตื้อ Mixer จนกว่าจะยอมตื่น!
        if (!isAudioInitialized && mainMixer != null)
        {
            float savedMusic = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
            float savedSFX = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

            if (savedMusic <= 0.001f) savedMusic = 0.75f;
            if (savedSFX <= 0.001f) savedSFX = 0.75f;

            float dbMusic = Mathf.Log10(savedMusic) * 20f;
            float dbSFX = Mathf.Log10(savedSFX) * 20f;

            bool isMixerReady = mainMixer.SetFloat("MusicVol", dbMusic);

            if (isMixerReady)
            {
                mainMixer.SetFloat("SFXVol", dbSFX);
                if (musicSlider != null) musicSlider.value = savedMusic;
                if (sfxSlider != null) sfxSlider.value = savedSFX;
                isAudioInitialized = true;
            }
        }

        // ระบบกดปุ่ม Esc เปิดหน้าต่างของคุณ
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
        UpdateSaveUI(); // ⭐ อัปเดตข้อความตอนเปิดหน้าต่าง
    }

    public void ShowLoadMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        UpdateSaveUI(); // ⭐ อัปเดตข้อความตอนเปิดหน้าต่าง
    }

    public void ShowSettingsMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    // ==========================================
    // 🔊 ระบบตั้งค่าเสียง
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
    // 💾 ระบบเซฟและโหลด 
    // ==========================================
    public void SaveGame(int slotNumber)
    {
        // ⭐ 1. ด่านตรวจที่ 1: เช็คก่อนว่าช่องนี้มีเซฟทับอยู่แล้วหรือเปล่า? 
        // (กันผู้เล่นมือลั่นกดซ้ำ แล้วเสียโควต้าฟรี)
        if (PlayerPrefs.HasKey("SaveSlot_" + slotNumber))
        {
            Debug.Log("⚠️ ช่องเซฟที่ " + slotNumber + " มีข้อมูลอยู่แล้ว! ต้องกดลบทิ้งก่อนถึงจะเซฟใหม่ได้");
            return; // เด้งออกเลย ไม่ทำการเซฟ และไม่หักโควต้า
        }

        // ⭐ 2. ด่านตรวจที่ 2: เช็คโควต้าว่าเซฟครบ 3 ครั้งหรือยัง?
        if (currentSaveCount >= maxSavesAllowed)
        {
            Debug.Log("❌ เซฟไม่ได้แล้ว! โควต้าครบ 3 ครั้งแล้ว");
            return;
        }

        // ---- ถ้าผ่านด่านตรวจมาได้ ก็เริ่มทำการเซฟปกติ ----
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

        // หักโควต้าเซฟ
        currentSaveCount++;
        PlayerPrefs.SetInt("SaveCount", currentSaveCount);
        PlayerPrefs.Save();

        Debug.Log($"✅ เซฟเกมลงช่อง {slotNumber} สำเร็จ!");

        UpdateSaveUI(); // อัปเดตข้อความทันทีหลังเซฟเสร็จ
        ShowLoadMenu(); // (ออปชันเสริม) เซฟเสร็จแล้วเด้งไปหน้าโหลด
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

    // ==========================================
    // 🗑️ ระบบลบทิ้ง และอัปเดตหน้าต่างข้อความ UI
    // ==========================================
    public void UpdateSaveUI()
    {
        // 1. อัปเดตข้อความของแต่ละช่องเซฟ
        for (int i = 0; i < slotTexts.Length; i++)
        {
            int slotNumber = i + 1; // อ้างอิงช่อง 1, 2, 3

            // ป้องกัน Error กรณีลืมใส่ Text ลงใน Inspector
            if (slotTexts[i] == null) continue;

            if (PlayerPrefs.HasKey("SaveSlot_" + slotNumber))
            {
                slotTexts[i].text = "ช่องเซฟ " + slotNumber + " : มีข้อมูล 💾";
                slotTexts[i].color = new Color(0.2f, 0.8f, 0.2f); // สีเขียว
            }
            else
            {
                slotTexts[i].text = "ช่องเซฟ " + slotNumber + " : ว่างเปล่า";
                slotTexts[i].color = Color.white; // สีขาว
            }
        }

        // 2. อัปเดตข้อความบอกโควต้า
        if (quotaText != null)
        {
            int savesLeft = maxSavesAllowed - currentSaveCount;
            quotaText.text = "สิทธิ์เซฟเกมเหลือ: " + savesLeft + " / " + maxSavesAllowed + " ครั้ง";

            // ถ้าโควต้าหมด ให้ตัวหนังสือเป็นสีแดงเตือนผู้เล่น
            if (savesLeft <= 0) quotaText.color = Color.red;
            else quotaText.color = Color.white;
        }
    }

    public void DeleteSave(int slotNumber)
    {
        string key = "SaveSlot_" + slotNumber;

        // เช็คว่ามีเซฟในช่องนี้จริงๆ ถึงจะยอมให้ลบ
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);

            // คืนโควต้าเซฟให้ 1 ครั้ง
            currentSaveCount--;
            if (currentSaveCount < 0) currentSaveCount = 0; // กันบั๊กติดลบ

            PlayerPrefs.SetInt("SaveCount", currentSaveCount);
            PlayerPrefs.Save();

            Debug.Log("🗑️ ลบข้อมูลช่อง " + slotNumber + " และคืนโควต้าให้แล้ว!");

            // สั่งอัปเดตหน้าจอทันที
            UpdateSaveUI();
        }
    }

    public void ResetSaveQuota()
    {
        currentSaveCount = 0;
        PlayerPrefs.SetInt("SaveCount", 0);
        PlayerPrefs.Save();
        UpdateSaveUI();
    }
}