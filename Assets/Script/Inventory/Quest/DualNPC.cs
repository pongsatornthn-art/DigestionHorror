using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class DualNPC : MonoBehaviour
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

        // ⭐ [เพิ่มใหม่] บทพูดเฉพาะของเควสนี้
        [Header("--- บทพูดก่อนเริ่มเควสนี้ ---")]
        [TextArea(2, 5)]
        public string[] questDialogues;

        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool hasAccepted = false;
    }

    [Header("NPC Basic Info")]
    public string npcName = "Mysterious Merchant";
    public Sprite portrait;

    [Header("--- Dialogue System ---")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public DialogueTypewriter typewriter;

    // เปลี่ยนมาใช้ตัวแปรซ่อน เพื่อรับค่าจากเควสแทน
    private string[] currentConversationLines;
    private int currentLine = 0;
    private bool isTalking = false;

    [Header("--- Shop Settings ---")]
    public List<ItemData> itemsForSale;

    [Header("--- Quest Settings ---")]
    public List<QuestData> quests = new List<QuestData>();
    private int currentQuestIndex = 0;

    [Header("--- UI Selection Panel ---")]
    public GameObject selectionPanel;
    public Button shopButton;
    public Button questButton;
    public Button closeButton;

    [Header("--- General Settings ---")]
    public float interactDistance = 3f;
    public GameObject allQuestsDoneUI;

    private Transform player;
    private bool isShowingUI = false;

    void Start()
    {
        if (PlayerController.instance != null) player = PlayerController.instance.transform;

        foreach (var q in quests)
        {
            if (q.startQuestUI != null) q.startQuestUI.SetActive(false);
            if (q.successUI != null) q.successUI.SetActive(false);
        }

        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (allQuestsDoneUI != null) allQuestsDoneUI.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(OpenShopLogic);
        }
        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(HandleQuestLogic);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSelectionPanel);
        }
    }

    void Update()
    {
        if (player == null)
        {
            if (PlayerController.instance != null) player = PlayerController.instance.transform;
            return;
        }

        if (isShowingUI && !isTalking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            if (!isTalking && !isShowingUI) StartConversation();
            else if (isTalking) NextDialogueLine();
        }
    }

    // ⭐ ดึงบทพูดของเควสปัจจุบันมาเล่น
    void StartConversation()
    {
        // เช็คว่ายังมีเควสเหลือไหม
        if (currentQuestIndex < quests.Count)
        {
            // ดึงบทพูดจากเควสปัจจุบันมาโหลดใส่
            currentConversationLines = quests[currentQuestIndex].questDialogues;
        }
        else
        {
            // ถ้าทำเควสหมดแล้ว ให้พูดประโยคนี้แทน
            currentConversationLines = new string[] { "Traveling Merchant : ไม่มีอะไรให้ทำแล้ว แวะมาแลกของได้อย่างเดียวนะไอ้หนุ่ม" };
        }

        // ถ้าไม่ได้ใส่บทพูดไว้ ให้ข้ามไปเปิดหน้าเมนูเลย
        if (currentConversationLines == null || currentConversationLines.Length == 0)
        {
            OpenSelectionPanel();
            return;
        }

        isTalking = true;
        currentLine = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        if (typewriter != null) typewriter.PlayDialogue(currentConversationLines[currentLine]);
        else if (dialogueText != null) dialogueText.text = currentConversationLines[currentLine];
    }

    void NextDialogueLine()
    {
        currentLine++;

        if (currentLine < currentConversationLines.Length)
        {
            if (typewriter != null) typewriter.PlayDialogue(currentConversationLines[currentLine]);
            else if (dialogueText != null) dialogueText.text = currentConversationLines[currentLine];
        }
        else
        {
            isTalking = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            OpenSelectionPanel();
        }
    }

    void OpenSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            isShowingUI = true;
            if (questButton != null) questButton.interactable = currentQuestIndex < quests.Count;
        }
    }

    public void CloseSelectionPanel()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        isShowingUI = false;
    }

    void OpenShopLogic()
    {
        CloseSelectionPanel();
        if (ShopUI.instance != null) ShopUI.instance.OpenShop(this.gameObject.GetComponent<ShopNPC_Helper>());
        if (InventoryUI.instance != null)
        {
            if (InventoryUI.instance.inventoryPanel != null) InventoryUI.instance.inventoryPanel.SetActive(true);
            if (InventoryUI.instance.craftingPanel != null) InventoryUI.instance.craftingPanel.SetActive(false);
        }
    }

    void HandleQuestLogic()
    {
        CloseSelectionPanel();
        if (currentQuestIndex >= quests.Count) return;

        QuestData currentQuest = quests[currentQuestIndex];

        if (!currentQuest.hasAccepted)
        {
            currentQuest.hasAccepted = true;
            StartCoroutine(ShowQuestUI(currentQuest.startQuestUI));
            return;
        }

        if (Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount)) CompleteCurrentQuest(currentQuest);
        else StartCoroutine(ShowQuestUI(currentQuest.startQuestUI));
    }

    void CompleteCurrentQuest(QuestData quest)
    {
        quest.isCompleted = true;
        if (quest.questType == QuestType.Consumable) Inventory.instance.RemoveItem(quest.questItem, quest.requiredAmount);
        if (BookUI.instance != null) BookUI.instance.UnlockNewPage(quest.pageToUnlock);

        StartCoroutine(ShowQuestUI(quest.successUI));
        currentQuestIndex++;

        if (currentQuestIndex >= quests.Count && allQuestsDoneUI != null) allQuestsDoneUI.SetActive(true);
    }

    IEnumerator ShowQuestUI(GameObject uiObject)
    {
        if (uiObject != null)
        {
            isShowingUI = true;
            uiObject.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            uiObject.SetActive(false);
            isShowingUI = false;
        }
    }
}