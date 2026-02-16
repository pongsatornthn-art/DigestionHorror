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
    // รายการเช็คว่าหน้าไหนปลดล็อกแล้วบ้าง (เช็คถูกใน Inspector เพื่อปลดหน้าเริ่มต้น)
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
        // ตั้งค่าเริ่มต้นให้ปลดล็อกหน้าแรกเสมอ (ถ้ายังไม่ได้ตั้งค่ามา)
        if (unlockedPages.Count < pages.Length)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (unlockedPages.Count <= i) unlockedPages.Add(false);
            }
        }
        unlockedPages[0] = true; // หน้าแรกปลดล็อกเสมอ
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

    // ✅ ฟังก์ชันใหม่สำหรับ NPC เรียกใช้เพื่อปลดล็อกหน้า
    public void UnlockNewPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < unlockedPages.Count)
        {
            unlockedPages[pageIndex] = true;
            // บังคับอัปเดตปุ่มทันที เผื่อผู้เล่นเปิดสมุดค้างไว้
            UpdatePageDisplay();
        }
    }

    public void OpenBook()
    {
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
        // ✅ ปรับปรุง: ไปหน้าถัดไปได้ต่อเมื่อหน้านั้นถูกปลดล็อกแล้วเท่านั้น
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

        // ✅ ปุ่มถัดไปจะกดได้ต่อเมื่อ "หน้าถัดไปถูกปลดล็อกแล้ว"
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