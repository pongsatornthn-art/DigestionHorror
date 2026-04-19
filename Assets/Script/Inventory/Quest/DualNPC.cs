using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DualNPC : MonoBehaviour
{
    public enum QuestType { Consumable, NonConsumable, TalkOnly }

    [System.Serializable]
    public class QuestData
    {
        public string questName;
        public QuestType questType;

        [Header("เงื่อนไขการส่งเควส (ถ้าเป็น TalkOnly ไม่ต้องใส่)")]
        public ItemData questItem;
        public int requiredAmount = 1;

        [Header("ของรางวัล (เมื่อส่งเควสสำเร็จ)")]
        public ItemData rewardItem;
        public int rewardAmount = 1;

        [Header("หน้าที่ต้องการปลดล็อก (ตอนส่งเควสสำเร็จ!)")] // ⭐
        public int pageToUnlock = 1;

        [Header("บทสนทนาประจำเควสนี้")]
        [TextArea] public string questGreeting = "ยินดีต้อนรับ! มีอะไรให้ข้าช่วยไหม?";
        [TextArea] public string questStartDialogue = "เจ้าช่วยหาของสิ่งนี้มาให้ข้าหน่อยได้ไหม?";
        [TextArea] public string questSuccessDialogue = "โอ้! ขอบใจเจ้ามาก นี่คือรางวัลของเจ้า!";

        [Header("เหตุการณ์เนื้อเรื่อง (Events)")]
        public UnityEvent onQuestAccepted;
        public UnityEvent onQuestCompleted;

        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool hasAccepted = false;
        [HideInInspector] public bool hasGreetedThisQuest = false;
    }

    [Header("NPC Basic Info")]
    public string npcName = "Mysterious Merchant";
    public Sprite portrait;
    [TextArea] public string fallbackGreeting = "ไม่มีอะไรให้เจ้าทำแล้วล่ะไอ้หนุ่ม...";

    [Header("Dialogue UI Settings")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public float typingSpeed = 0.05f;

    [Header("Shop Settings")]
    public List<ItemData> itemsForSale;

    [Header("Quest Settings")]
    public List<QuestData> quests = new List<QuestData>();
    public int currentQuestIndex = 0;

    [Header("Boss Settings (Endgame)")]
    public GameObject bossPlaceholder;

    [Header("UI Selection Panel")]
    public GameObject selectionPanel;
    public Button shopButton;
    public Button questButton;
    public Button closeButton;

    [Header("General Settings")]
    public float interactDistance = 3f;
    public GameObject allQuestsDoneUI;

    private Transform player;
    private bool isShowingUI = false;
    private Coroutine typingCoroutine;
    private bool fallbackHasGreeted = false;

    void Start()
    {
        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;

        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (allQuestsDoneUI != null) allQuestsDoneUI.SetActive(false);

        SyncBossState();

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
            closeButton.onClick.AddListener(CloseEverything);
        }
    }

    void Update()
    {
        if (player == null)
        {
            if (PlayerController.instance != null) player = PlayerController.instance.transform;
            return;
        }

        if (isShowingUI) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    public void SyncBossState()
    {
        if (bossPlaceholder != null)
        {
            if (currentQuestIndex >= quests.Count)
            {
                bossPlaceholder.SetActive(true);
                Debug.Log("🔥 เควสหมดแล้ว! ทำการเสก Boss Placeholder ออกมา!");
            }
            else
            {
                bossPlaceholder.SetActive(false);
            }
        }
    }

    void StartDialogue()
    {
        isShowingUI = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (selectionPanel != null) selectionPanel.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        string textToSay = "";
        bool shouldSkip = false;

        if (currentQuestIndex < quests.Count)
        {
            textToSay = quests[currentQuestIndex].questGreeting;
            shouldSkip = quests[currentQuestIndex].hasGreetedThisQuest;
        }
        else
        {
            textToSay = fallbackGreeting;
            shouldSkip = fallbackHasGreeted;
        }

        typingCoroutine = StartCoroutine(TypeDialogueCoroutine(textToSay, true, shouldSkip));
    }

    IEnumerator TypeDialogueCoroutine(string textToType, bool isGreeting, bool skipTyping = false)
    {
        string fullText = textToType;
        yield return new WaitForSeconds(0.1f);

        if (dialogueText != null)
        {
            if (skipTyping)
            {
                dialogueText.text = fullText;
                yield return StartCoroutine(ForceRefreshUI());
            }
            else
            {
                dialogueText.text = "";
                foreach (char letter in fullText.ToCharArray())
                {
                    dialogueText.text += letter;
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                    {
                        dialogueText.text = fullText;
                        yield return StartCoroutine(ForceRefreshUI());
                        yield return new WaitForSeconds(0.2f);
                        break;
                    }
                    yield return new WaitForSeconds(typingSpeed);
                }
            }
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space));
        yield return new WaitForSeconds(0.1f);

        if (isGreeting)
        {
            if (currentQuestIndex < quests.Count)
                quests[currentQuestIndex].hasGreetedThisQuest = true;
            else
                fallbackHasGreeted = true;

            OpenSelectionPanel();
        }
        else
        {
            CloseEverything();
        }
    }

    IEnumerator ForceRefreshUI()
    {
        yield return new WaitForEndOfFrame();
        if (dialogueText != null)
        {
            dialogueText.ForceMeshUpdate();
            RectTransform textRect = dialogueText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
                if (textRect.parent != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(textRect.parent.GetComponent<RectTransform>());
                }
            }
        }
        Canvas.ForceUpdateCanvases();
    }

    void OpenSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            if (questButton != null)
                questButton.interactable = currentQuestIndex < quests.Count;
        }
    }

    public void CloseEverything()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        isShowingUI = false;
    }

    void OpenShopLogic()
    {
        CloseEverything();
        if (ShopUI.instance != null)
            ShopUI.instance.OpenShop(this.gameObject.GetComponent<ShopNPC_Helper>());

        if (InventoryUI.instance != null)
        {
            if (InventoryUI.instance.inventoryPanel != null)
                InventoryUI.instance.inventoryPanel.SetActive(true);

            if (InventoryUI.instance.craftingPanel != null)
                InventoryUI.instance.craftingPanel.SetActive(false);
        }
    }

    void HandleQuestLogic()
    {
        if (currentQuestIndex >= quests.Count) return;
        QuestData currentQuest = quests[currentQuestIndex];

        if (selectionPanel != null) selectionPanel.SetActive(false);
        isShowingUI = true;

        if (!currentQuest.hasAccepted)
        {
            currentQuest.hasAccepted = true;
            currentQuest.onQuestAccepted?.Invoke();

            // ⭐ เอาการปลดล็อกสมุดออกจากตรงนี้แล้ว

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(currentQuest.questStartDialogue, false));
            return;
        }

        bool canComplete = false;

        if (currentQuest.questType == QuestType.TalkOnly)
        {
            canComplete = true;
        }
        else
        {
            if (Inventory.instance != null && Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount))
            {
                canComplete = true;
            }
        }

        if (canComplete)
        {
            CompleteCurrentQuest(currentQuest);
        }
        else
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(currentQuest.questStartDialogue, false));
        }
    }

    void CompleteCurrentQuest(QuestData quest)
    {
        quest.isCompleted = true;

        if (quest.questType == QuestType.Consumable)
        {
            if (Inventory.instance != null)
                Inventory.instance.RemoveItem(quest.questItem, quest.requiredAmount);
        }

        if (quest.rewardItem != null && Inventory.instance != null)
        {
            for (int i = 0; i < quest.rewardAmount; i++)
            {
                Inventory.instance.AddItem(quest.rewardItem);
            }
        }

        // ⭐ ย้ายคำสั่งปลดล็อกสมุดกลับมาไว้ที่นี่! (ปลดล็อกตอนส่งเควสสำเร็จ)
        if (BookUI.instance != null)
        {
            BookUI.instance.UnlockNewPage(quest.pageToUnlock);
            Debug.Log("📖 ส่งเควสสำเร็จ! ปลดล็อกสมุดหน้าที่: " + quest.pageToUnlock + " แล้ว!");
        }

        quest.onQuestCompleted?.Invoke();

        currentQuestIndex++;

        if (currentQuestIndex >= quests.Count)
        {
            if (allQuestsDoneUI != null)
                allQuestsDoneUI.SetActive(true);
        }

        SyncBossState();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogueCoroutine(quest.questSuccessDialogue, false));
    }
}