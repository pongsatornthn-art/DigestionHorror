using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestNPC : MonoBehaviour
{
    public enum QuestType { Consumable, NonConsumable }

    [System.Serializable]
    public class QuestData
    {
        public string questName;
        public QuestType questType;
        public ItemData questItem;
        public int requiredAmount = 1;
        public int pageToUnlock = 1;
        public GameObject startQuestUI;
        public GameObject successUI;
        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool hasAccepted = false;
    }

    [Header("Quest List")]
    public List<QuestData> quests = new List<QuestData>();
    private int currentQuestIndex = 0;

    [Header("Settings")]
    public float interactDistance = 3f;
    public GameObject allQuestsDoneUI;

    private Transform player;
    private bool isMouseOver = false;

    // ⭐ เพิ่มตัวแปรนี้เพื่อป้องกันผู้เล่นกด 'E' รัวๆ จน UI ซ้อนกัน
    private bool isShowingUI = false;

    void Start()
    {
        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;

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
        // ถ้ากำลังโชว์ UI อยู่ จะไม่อนุญาตให้กดอะไรเพิ่มจนกว่า UI จะดับไป
        if (currentQuestIndex >= quests.Count || player == null || isShowingUI) return;

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

        if (Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount))
        {
            CompleteCurrentQuest(currentQuest);
        }
        else
        {
            StartCoroutine(ShowUI(currentQuest.startQuestUI));
        }
    }

    void CompleteCurrentQuest(QuestData quest)
    {
        quest.isCompleted = true;

        if (quest.questType == QuestType.Consumable)
        {
            Inventory.instance.RemoveItem(quest.questItem, quest.requiredAmount);
        }

        if (BookUI.instance != null)
            BookUI.instance.UnlockNewPage(quest.pageToUnlock);

        StartCoroutine(ShowUI(quest.successUI));

        Debug.Log("เควสสำเร็จ! ปลดล็อกหน้า: " + quest.pageToUnlock);

        currentQuestIndex++;

        if (currentQuestIndex >= quests.Count && allQuestsDoneUI != null)
        {
            Debug.Log("ทำครบทุกเควสของ NPC ตัวนี้แล้ว");
        }
    }

    // ⭐ ปรับปรุงระบบโชว์ UI ให้ล็อกการกระทำของผู้เล่นชั่วคราว
    IEnumerator ShowUI(GameObject uiObject)
    {
        if (uiObject != null)
        {
            isShowingUI = true; // ล็อกไม่ให้กด E ซ้ำ
            uiObject.SetActive(true);

            yield return new WaitForSeconds(2.5f); // รอ 2.5 วินาที

            uiObject.SetActive(false);
            isShowingUI = false; // ปลดล็อกให้กดคุยต่อได้
        }
    }
}