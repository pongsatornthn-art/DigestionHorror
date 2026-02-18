using UnityEngine;
using System.Collections;
using System.Collections.Generic; // เพิ่มเพื่อให้ใช้ List ได้

public class QuestNPC : MonoBehaviour
{
    public enum QuestType { Consumable, NonConsumable }

    [System.Serializable]
    public class QuestData // สร้างโครงสร้างข้อมูลเควส
    {
        public string questName; // ตั้งชื่อเควสให้จำง่าย
        public QuestType questType;
        public ItemData questItem;
        public int requiredAmount = 1;
        public int pageToUnlock = 1;
        public GameObject startQuestUI; // UI บอกว่าต้องทำอะไร
        public GameObject successUI;    // UI เมื่อทำสำเร็จ
        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool hasAccepted = false;
    }

    [Header("Quest List")]
    public List<QuestData> quests = new List<QuestData>(); // เก็บรายการเควสทั้งหมด
    private int currentQuestIndex = 0; // ลำดับเควสปัจจุบัน

    [Header("Settings")]
    public float interactDistance = 3f;
    public GameObject allQuestsDoneUI; // UI เมื่อทำครบทุกเควสแล้ว (ถ้ามี)

    private Transform player;
    private bool isMouseOver = false;

    void Start()
    {
        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;

        // ปิด UI ทั้งหมดก่อน
        foreach (var q in quests)
        {
            if (q.startQuestUI != null) q.startQuestUI.SetActive(false);
            if (q.successUI != null) q.successUI.SetActive(false);
        }
        if (allQuestsDoneUI != null) allQuestsDoneUI.SetActive(false);
    }

    void OnMouseEnter() { isMouseOver = true; }
    void OnMouseExit() { isMouseOver = false; }

    void Update()
    {
        // ถ้าทำครบทุกเควสแล้ว หรือไม่มีผู้เล่น ให้หยุดทำงาน
        if (currentQuestIndex >= quests.Count || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= interactDistance && isMouseOver && Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        QuestData currentQuest = quests[currentQuestIndex];

        if (!currentQuest.hasAccepted)
        {
            currentQuest.hasAccepted = true;
            StartCoroutine(ShowUI(currentQuest.startQuestUI));
            Debug.Log("รับเควส: " + currentQuest.questName);
            return;
        }

        // เช็คไอเทมใน Inventory ผ่าน Inventory.instance
        if (Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount))
        {
            CompleteCurrentQuest(currentQuest);
        }
        else
        {
            // ถ้ายังมีไอเทมไม่ครบ ให้โชว์ UI เควสเดิมซ้ำ
            StartCoroutine(ShowUI(currentQuest.startQuestUI));
        }
    }

    void CompleteCurrentQuest(QuestData quest)
    {
        quest.isCompleted = true;

        // ถ้าเป็นแบบใช้แล้วหมดไป ให้ลบไอเทม
        if (quest.questType == QuestType.Consumable)
        {
            Inventory.instance.RemoveItem(quest.questItem, quest.requiredAmount);
        }

        // ปลดล็อกหน้าหนังสือผ่าน BookUI.instance
        if (BookUI.instance != null)
            BookUI.instance.UnlockNewPage(quest.pageToUnlock);

        StartCoroutine(ShowUI(quest.successUI));

        Debug.Log("เควสสำเร็จ! ปลดล็อกหน้า: " + quest.pageToUnlock);

        // เลื่อนไปเควสถัดไป
        currentQuestIndex++;

        // ถ้าทำครบทุกเควสแล้ว
        if (currentQuestIndex >= quests.Count && allQuestsDoneUI != null)
        {
            Debug.Log("ทำครบทุกเควสของ NPC ตัวนี้แล้ว");
        }
    }

    IEnumerator ShowUI(GameObject uiObject)
    {
        if (uiObject != null)
        {
            uiObject.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            uiObject.SetActive(false);
        }
    }
}