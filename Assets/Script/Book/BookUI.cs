using UnityEngine;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bookPanel;  // ตัวหน้าต่างหนังสือ
    public GameObject[] pages;    // อาเรย์เก็บหน้ากระดาษทั้งหมด (Page1, Page2...)

    [Header("Buttons")]
    public Button nextButton;     // ปุ่มหน้าถัดไป
    public Button prevButton;     // ปุ่มหน้าก่อน

    [Header("Audio Settings")]
    public AudioSource audioSource; // ตัวเล่นเสียง
    public AudioClip pageTurnSound; // ไฟล์เสียงเปิดกระดาษ (ฟรึ่บ!)

    private int currentPageIndex = 0; // หน้าปัจจุบันอยู่ที่ไหน

    void Start()
    {
        // เริ่มเกมมาให้ปิดหนังสือ
        CloseBook();

        // สั่งให้ปุ่มทำงานเมื่อถูกกด
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
    }

    void Update()
    {
        // กด J เพื่อเปิด/ปิด
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (bookPanel.activeSelf)
                CloseBook();
            else
                OpenBook();
        }
    }

    // ==========================================
    // ฟังก์ชันหลัก
    // ==========================================

    public void OpenBook()
    {
        bookPanel.SetActive(true);
        currentPageIndex = 0; // เปิดมาเริ่มที่หน้าแรกเสมอ
        UpdatePageDisplay();

        // (Optional) หยุดเวลาเกม หรือซ่อนเมาส์ ถ้าต้องการ
        // Time.timeScale = 0; 
    }

    public void CloseBook()
    {
        bookPanel.SetActive(false);
        // Time.timeScale = 1; // คืนเวลาเกม
    }

    public void NextPage()
    {
        // ถ้ายังไม่ใช่หน้าสุดท้าย -> ไปหน้าถัดไปได้
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
            PlaySound();
        }
    }

    public void PrevPage()
    {
        // ถ้ายังไม่ใช่หน้าแรก -> ถอยกลับได้
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
            PlaySound();
        }
    }

    // ฟังก์ชันอัปเดตหน้าจอ (โชว์เฉพาะหน้าปัจจุบัน)
    void UpdatePageDisplay()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            // ถ้า i ตรงกับหน้าปัจจุบัน ให้ SetActive(true) ที่เหลือปิดหมด
            if (i == currentPageIndex)
                pages[i].SetActive(true);
            else
                pages[i].SetActive(false);
        }

        // อัปเดตปุ่ม (ถ้าอยู่หน้าแรก ปิดปุ่มถอย / ถ้าอยู่หน้าท้าย ปิดปุ่มไปต่อ)
        if (prevButton) prevButton.interactable = (currentPageIndex > 0);
        if (nextButton) nextButton.interactable = (currentPageIndex < pages.Length - 1);
    }

    void PlaySound()
    {
        if (audioSource != null && pageTurnSound != null)
        {
            // สุ่มเสียง Pitch นิดหน่อยให้ดูสมจริง (ไม่ซ้ำซาก)
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(pageTurnSound);
        }
    }
}