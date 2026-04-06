using UnityEngine;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string instructionText;
    public KeyCode keyToPress;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI Settings")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;

    [Header("Tutorial Steps (ตั้งค่าลำดับการสอน)")]
    public TutorialStep[] steps;

    [Header("🛠️ Debug (โหมดเทสเกม)")]
    [Tooltip("ติ๊กถูกช่องนี้ ถ้าอยากให้หน้าต่างสอนเล่นเด้งขึ้นมา 'ทุกครั้ง' ที่กด Play")]
    public bool forceShowTutorial = false; // ⭐ เพิ่มตัวแปรนี้เข้ามา

    private int currentStepIndex = 0;
    private bool isTutorialActive = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // ⭐ เช็คว่าเปิดโหมดบังคับโชว์อยู่ไหม?
        if (forceShowTutorial)
        {
            Debug.Log("🛠️ [Debug Mode] บังคับเปิดระบบสอนเล่น!");
            StartTutorial();
            return; // หยุดการทำงานตรงนี้เลย ไม่ต้องไปเช็ค PlayerPrefs
        }

        // ระบบปกติ: เช็คว่าเคยผ่านโหมดสอนเล่นมาหรือยัง (0 = ยังไม่เคย, 1 = เคยแล้ว)
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
            if (tutorialText != null)
            {
                tutorialText.text = steps[currentStepIndex].instructionText;
            }
        }
        else
        {
            EndTutorial();
        }
    }

    void Update()
    {
        if (!isTutorialActive) return;

        if (Input.GetKeyDown(steps[currentStepIndex].keyToPress))
        {
            currentStepIndex++;
            ShowCurrentStep();
        }
    }

    void EndTutorial()
    {
        isTutorialActive = false;
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        // บันทึกลงเครื่อง (จะบันทึกก็ต่อเมื่อไม่ได้เปิดโหมด Force Show)
        if (!forceShowTutorial)
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }

        Debug.Log("✅ จบโหมดสอนเล่น!");
    }

    // ⭐ เพิ่ม ContextMenu ให้คลิกขวาที่สคริปต์เพื่อรีเซ็ตได้เลย
    [ContextMenu("🔄 รีเซ็ตความจำโหมดสอนเล่น (Clear Save)")]
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("🔄 รีเซ็ตโหมดสอนเล่นแล้ว (ถ้าไม่ได้ติ๊ก Force Show เริ่มเกมรอบหน้าก็จะเด้งใหม่ครับ)");
    }
}