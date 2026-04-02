using UnityEngine;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string instructionText; // ข้อความที่จะให้แสดง เช่น "กด J เพื่ออ่านเอกสาร"
    public KeyCode keyToPress;     // ปุ่มที่ต้องกดให้ผ่าน เช่น KeyCode.J
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI Settings")]
    public GameObject tutorialPanel;   // กรอบ UI แจ้งเตือน
    public TextMeshProUGUI tutorialText; // ตัวหนังสือบอกให้กด

    [Header("Tutorial Steps (ตั้งค่าลำดับการสอน)")]
    public TutorialStep[] steps;

    private int currentStepIndex = 0;
    private bool isTutorialActive = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // เช็คว่าเคยผ่านโหมดสอนเล่นมาหรือยัง (0 = ยังไม่เคย, 1 = เคยแล้ว)
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
        {
            StartTutorial();
        }
        else
        {
            // ถ้าเคยเล่นแล้ว ให้ซ่อน UI สอนไปเลย
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
        }
    }

    public void StartTutorial()
    {
        if (steps.Length == 0) return;

        isTutorialActive = true;
        currentStepIndex = 0;

        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        ShowCurrentStep();
    }

    void ShowCurrentStep()
    {
        if (currentStepIndex < steps.Length)
        {
            // แสดงข้อความของสเต็ปปัจจุบัน
            if (tutorialText != null)
            {
                tutorialText.text = steps[currentStepIndex].instructionText;
            }
        }
        else
        {
            // ถ้าทำครบทุกสเต็ปแล้ว ให้จบการสอน
            EndTutorial();
        }
    }

    void Update()
    {
        if (!isTutorialActive) return;

        // เช็คว่าผู้เล่นกดปุ่มตรงกับที่สั่งไว้ในสเต็ปนี้หรือเปล่า
        if (Input.GetKeyDown(steps[currentStepIndex].keyToPress))
        {
            currentStepIndex++; // เปลี่ยนไปสเต็ปถัดไป
            ShowCurrentStep();  // อัปเดตข้อความบนจอ
        }
    }

    void EndTutorial()
    {
        isTutorialActive = false;
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        // บันทึกลงเครื่องไว้ว่า "ผ่านการสอนเล่นแล้วนะ" รอบหน้าจะได้ไม่เด้งอีก
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        Debug.Log("✅ จบโหมดสอนเล่น!");
    }

    // ⭐ แถมฟังก์ชันสำหรับปุ่ม Reset ให้เผื่อเอาไว้ใช้เทสเกม
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("🔄 รีเซ็ตโหมดสอนเล่นแล้ว (เริ่มเกมรอบหน้าจะเด้งใหม่)");
    }
}