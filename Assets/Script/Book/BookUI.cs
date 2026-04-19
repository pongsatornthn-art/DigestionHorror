using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BookUI : MonoBehaviour
{
    public static BookUI instance; // ทำเป็น Singleton เพื่อให้ NPC เรียกใช้ง่ายๆ

    [Header("UI References")]
    public GameObject bookPanel;
    public GameObject[] pages;

    [Header("Unlock Settings")]
    public List<bool> unlockedPages = new List<bool>();

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip pageTurnSound;

    private int currentPageIndex = 0;

    void Awake()
    {
        instance = this;
        // ตั้งค่าเริ่มต้นให้ปลดล็อกหน้าแรกเสมอ
        if (unlockedPages.Count < pages.Length)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (unlockedPages.Count <= i) unlockedPages.Add(false);
            }
        }
        unlockedPages[0] = true;
    }

    void Start()
    {
        CloseBook();
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (bookPanel.activeSelf) CloseBook();
            else OpenBook();
        }
    }

    public void UnlockNewPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < unlockedPages.Count)
        {
            unlockedPages[pageIndex] = true;
            UpdatePageDisplay();
        }
    }

    // ⭐ ฟังก์ชันใหม่: สแกนความคืบหน้าเควสแบบเรียลไทม์!
    public void ScanQuestProgress()
    {
        DualNPC[] allNPCs = FindObjectsByType<DualNPC>(FindObjectsSortMode.None);
        if (allNPCs.Length > 0)
        {
            DualNPC npc = allNPCs[0];

            // เช็คว่าถ้าเลยเควสที่ 3 ไปแล้ว (currentQuestIndex > 3) -> ให้ปลดล็อกทุกหน้า!
            if (npc.currentQuestIndex > 3)
            {
                for (int i = 0; i < unlockedPages.Count; i++)
                {
                    unlockedPages[i] = true;
                }
            }
            else
            {
                // ถ้ายังไม่เลยเควสที่ 3: ให้ปลดล็อกเฉพาะหน้าที่ทำเควสเสร็จแล้ว
                if (unlockedPages.Count > 0) unlockedPages[0] = true; // หน้าแรกปลดไว้เสมอ

                // ไล่เช็คประวัติเควสที่ "ทำสำเร็จแล้ว" (น้อยกว่าเควสปัจจุบัน)
                for (int i = 0; i < npc.currentQuestIndex; i++)
                {
                    int pageToUnlock = npc.quests[i].pageToUnlock;
                    if (pageToUnlock >= 0 && pageToUnlock < unlockedPages.Count)
                    {
                        unlockedPages[pageToUnlock] = true;
                    }
                }
            }
        }
    }

    public void OpenBook()
    {
        ScanQuestProgress(); // ⭐ สั่งให้สแกนเควสทุกครั้งที่หยิบสมุดขึ้นมาอ่าน!

        bookPanel.SetActive(true);
        currentPageIndex = 0;
        UpdatePageDisplay();
    }

    public void CloseBook()
    {
        bookPanel.SetActive(false);
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1 && unlockedPages[currentPageIndex + 1])
        {
            currentPageIndex++;
            UpdatePageDisplay();
            PlaySound();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
            PlaySound();
        }
    }

    void UpdatePageDisplay()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }

        if (prevButton) prevButton.interactable = (currentPageIndex > 0);

        // ปุ่มถัดไปจะกดได้ต่อเมื่อ "หน้าถัดไปถูกปลดล็อกแล้ว"
        if (nextButton)
        {
            bool hasNextPage = currentPageIndex < pages.Length - 1;
            bool isNextPageUnlocked = hasNextPage && unlockedPages[currentPageIndex + 1];
            nextButton.interactable = isNextPageUnlocked;
        }
    }

    void PlaySound()
    {
        if (audioSource != null && pageTurnSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(pageTurnSound);
        }
    }
}