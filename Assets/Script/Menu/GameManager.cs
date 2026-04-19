using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.IO;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public float playerPosX, playerPosY;
    public int health;
    public float stamina;
    public int money;

    public List<string> inventoryItemNames = new List<string>();
    public List<int> inventoryItemAmounts = new List<int>();

    public int currentQuestIndex;
    public bool isCurrentQuestAccepted;

    // ⭐ เพิ่มตรงนี้: ลิสต์สำหรับจำว่าหน้าสมุดไหนถูกปลดล็อกแล้วบ้าง
    public List<bool> unlockedBookPages = new List<bool>();
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

    [Header("Save UI Texts (ข้อความสถานะเซฟ)")]
    public TMP_Text[] slotTexts;
    public TMP_Text quotaText;

    private bool isPaused = false;
    private bool isAudioInitialized = false;

    void Awake()
    {
        instance = this;
        currentSaveCount = PlayerPrefs.GetInt("SaveCount", 0);
    }

    void Start()
    {
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        UpdateSaveUI();

        int slotToLoad = PlayerPrefs.GetInt("SlotToLoad", 0);

        if (slotToLoad > 0)
        {
            Debug.Log("🔄 รับคำสั่งจากหน้าเมนู! กำลังโหลดเซฟช่องที่: " + slotToLoad);
            LoadGame(slotToLoad);

            PlayerPrefs.SetInt("SlotToLoad", 0);
        }
    }

    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame() { isPaused = true; Time.timeScale = 0f; ShowPauseMainMenu(); }
    public void ResumeGame()
    {
        isPaused = false; Time.timeScale = 1f;
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
        UpdateSaveUI();
    }
    public void ShowLoadMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        UpdateSaveUI();
    }
    public void ShowSettingsMenu()
    {
        if (pauseMainPanel) pauseMainPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

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

    public void SaveGame(int slotNumber)
    {
        if (PlayerPrefs.HasKey("SaveSlot_" + slotNumber))
        {
            Debug.Log("⚠️ ช่องเซฟที่ " + slotNumber + " มีข้อมูลอยู่แล้ว! ต้องกดลบทิ้งก่อนถึงจะเซฟใหม่ได้");
            return;
        }

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

        if (Inventory.instance != null)
        {
            foreach (var slot in Inventory.instance.items)
            {
                if (slot != null && slot.itemData != null)
                {
                    data.inventoryItemNames.Add(slot.itemData.name);
                    data.inventoryItemAmounts.Add(slot.amount);
                }
                else
                {
                    data.inventoryItemNames.Add("");
                    data.inventoryItemAmounts.Add(0);
                }
            }
        }

        DualNPC[] allNPCs = FindObjectsByType<DualNPC>(FindObjectsSortMode.None);
        if (allNPCs.Length > 0)
        {
            DualNPC mainNPC = allNPCs[0];
            data.currentQuestIndex = mainNPC.currentQuestIndex;
            if (mainNPC.currentQuestIndex < mainNPC.quests.Count)
            {
                data.isCurrentQuestAccepted = mainNPC.quests[mainNPC.currentQuestIndex].hasAccepted;
            }
        }

        // ⭐ บันทึกข้อมูลสมุด
        if (BookUI.instance != null)
        {
            data.unlockedBookPages = new List<bool>(BookUI.instance.unlockedPages);
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveSlot_" + slotNumber, json);

        currentSaveCount++;
        PlayerPrefs.SetInt("SaveCount", currentSaveCount);
        PlayerPrefs.Save();

        Debug.Log($"✅ เซฟเกมลงช่อง {slotNumber} สำเร็จ! (รวมกระเป๋า เควส และสมุด)");

        UpdateSaveUI();
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

            if (Inventory.instance != null)
            {
                Inventory.instance.items.Clear();
                Inventory.instance.Unequip();

                for (int i = 0; i < data.inventoryItemNames.Count; i++)
                {
                    if (!string.IsNullOrEmpty(data.inventoryItemNames[i]))
                    {
                        ItemData loadedItem = Resources.Load<ItemData>("Items/" + data.inventoryItemNames[i]);

                        if (loadedItem != null)
                            Inventory.instance.items.Add(new InventoryItem(loadedItem, data.inventoryItemAmounts[i]));
                        else
                            Inventory.instance.items.Add(null);
                    }
                    else
                    {
                        Inventory.instance.items.Add(null);
                    }
                }

                while (Inventory.instance.items.Count < Inventory.instance.space)
                    Inventory.instance.items.Add(null);

                if (Inventory.instance.onItemChangedCallback != null)
                    Inventory.instance.onItemChangedCallback.Invoke();
            }

            DualNPC[] allNPCs = FindObjectsByType<DualNPC>(FindObjectsSortMode.None);
            if (allNPCs.Length > 0)
            {
                DualNPC mainNPC = allNPCs[0];
                mainNPC.currentQuestIndex = data.currentQuestIndex;

                foreach (var q in mainNPC.quests) { q.hasAccepted = false; q.isCompleted = false; q.hasGreetedThisQuest = false; }

                for (int i = 0; i < data.currentQuestIndex; i++)
                {
                    mainNPC.quests[i].isCompleted = true;
                    mainNPC.quests[i].hasAccepted = true;
                    mainNPC.quests[i].hasGreetedThisQuest = true;
                }

                if (data.currentQuestIndex < mainNPC.quests.Count)
                {
                    mainNPC.quests[data.currentQuestIndex].hasAccepted = data.isCurrentQuestAccepted;
                    if (data.isCurrentQuestAccepted)
                        mainNPC.quests[data.currentQuestIndex].hasGreetedThisQuest = true;
                }

                mainNPC.SyncBossState();
            }

            // ⭐ โหลดข้อมูลหน้าสมุดกลับมา
            if (BookUI.instance != null && data.unlockedBookPages != null && data.unlockedBookPages.Count > 0)
            {
                BookUI.instance.unlockedPages = new List<bool>(data.unlockedBookPages);

                // กันเหนียว: ให้แน่ใจว่าหน้าแรกสุดปลดล็อกอยู่เสมอ
                if (BookUI.instance.unlockedPages.Count > 0)
                {
                    BookUI.instance.unlockedPages[0] = true;
                }
            }

            Debug.Log($"📂 โหลดเกมช่อง {slotNumber} สำเร็จ! (ดึงกระเป๋า เควส และสมุดมาแล้ว)");
            ResumeGame();
        }
        else
        {
            Debug.Log("❌ ไม่มีข้อมูลเซฟในช่องนี้!");
        }
    }

    public void UpdateSaveUI()
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

        if (quotaText != null)
        {
            int savesLeft = maxSavesAllowed - currentSaveCount;
            quotaText.text = "สิทธิ์เซฟเกมเหลือ: " + savesLeft + " / " + maxSavesAllowed + " ครั้ง";

            if (savesLeft <= 0) quotaText.color = Color.red;
            else quotaText.color = Color.white;
        }
    }

    public void DeleteSave(int slotNumber)
    {
        string key = "SaveSlot_" + slotNumber;

        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);

            currentSaveCount--;
            if (currentSaveCount < 0) currentSaveCount = 0;

            PlayerPrefs.SetInt("SaveCount", currentSaveCount);
            PlayerPrefs.Save();

            Debug.Log("🗑️ ลบข้อมูลช่อง " + slotNumber + " และคืนโควต้าให้แล้ว!");
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