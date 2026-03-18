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

        [Header("บทสนทนาประจำเควสนี้")]
        [TextArea] public string questGreeting = "ยินดีต้อนรับ! มีอะไรให้ข้าช่วยไหม?";
        [TextArea] public string questStartDialogue = "เจ้าช่วยหาของสิ่งนี้มาให้ข้าหน่อยได้ไหม?";
        [TextArea] public string questSuccessDialogue = "โอ้! ขอบใจเจ้ามาก นี่คือรางวัลของเจ้า!";

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
    private int currentQuestIndex = 0;

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

        yield return new WaitForSeconds(0.1f); // กันบั๊กปุ่มลั่น

        if (dialogueText != null)
        {
            if (skipTyping)
            {
                dialogueText.text = fullText;
                yield return StartCoroutine(ForceRefreshUI()); // 🪄 เรียกใช้นิวเคลียร์!
            }
            else
            {
                dialogueText.text = ""; // เคลียร์ข้อความให้ว่าง

                foreach (char letter in fullText.ToCharArray())
                {
                    dialogueText.text += letter;

                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                    {
                        dialogueText.text = fullText;
                        yield return StartCoroutine(ForceRefreshUI()); // 🪄 เรียกใช้นิวเคลียร์!
                        yield return new WaitForSeconds(0.2f); // กันปุ่มลั่นเบิ้ล
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

    // 💣 นิวเคลียร์บังคับรีเฟรช UI (ลึกแค่ไหนก็ต้องขยาย!)
    IEnumerator ForceRefreshUI()
    {
        yield return new WaitForEndOfFrame();

        if (dialogueText != null)
        {
            dialogueText.ForceMeshUpdate(); // สั่ง Text ขยาย

            RectTransform textRect = dialogueText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                // สั่งกระดาษ (Text) ขยายเดี๋ยวนี้!
                LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);

                // สั่งถุง (Content) ขยายเดี๋ยวนี้!
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

            // สั่งปิดหน้าต่างคราฟต์กันบัง
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
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(currentQuest.questStartDialogue, false));
            return;
        }

        if (Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount))
            CompleteCurrentQuest(currentQuest);
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
            Inventory.instance.RemoveItem(quest.questItem, quest.requiredAmount);

        if (BookUI.instance != null)
            BookUI.instance.UnlockNewPage(quest.pageToUnlock);

        currentQuestIndex++;

        if (currentQuestIndex >= quests.Count && allQuestsDoneUI != null)
            allQuestsDoneUI.SetActive(true);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogueCoroutine(quest.questSuccessDialogue, false));
    }
}