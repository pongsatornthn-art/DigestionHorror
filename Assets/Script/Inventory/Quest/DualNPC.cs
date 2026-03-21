using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // ⭐ [เพิ่ม] ดึงไลบรารี UnityEvent มาใช้สำหรับสร้าง Event เนื้อเรื่อง

public class DualNPC : MonoBehaviour
{
    // ⭐ [อัปเกรด] เพิ่ม TalkOnly เข้ามา สำหรับเควสที่แค่เดินไปหา NPC แล้วกดคุยก็ผ่านเลย
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
        public ItemData rewardItem; // ⭐ ไอเทมที่จะได้รับ (เช่น สมุดบันทึก)
        public int rewardAmount = 1;
        public int pageToUnlock = 1;

        [Header("บทสนทนาประจำเควสนี้")]
        [TextArea] public string questGreeting = "ยินดีต้อนรับ! มีอะไรให้ข้าช่วยไหม?";
        [TextArea] public string questStartDialogue = "เจ้าช่วยหาของสิ่งนี้มาให้ข้าหน่อยได้ไหม?";
        [TextArea] public string questSuccessDialogue = "โอ้! ขอบใจเจ้ามาก นี่คือรางวัลของเจ้า!";

        [Header("เหตุการณ์เนื้อเรื่อง (Events)")]
        public UnityEvent onQuestAccepted;  // ⭐ เกิดขึ้นตอน "กดรับเควส" (เช่น เสกผี Watching Hour)
        public UnityEvent onQuestCompleted; // ⭐ เกิดขึ้นตอน "ส่งเควส" (เช่น เปิดหน้าต่างสกิล, เปิดประตู)

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

        // ถ้ายังไม่ได้กดรับเควส
        if (!currentQuest.hasAccepted)
        {
            currentQuest.hasAccepted = true;

            // ⭐ [อัปเกรด] สั่งทำงาน Event "ตอนรับเควส" ทันที!
            currentQuest.onQuestAccepted?.Invoke();

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(currentQuest.questStartDialogue, false));
            return;
        }

        bool canComplete = false;

        // ⭐ [อัปเกรด] ตรวจสอบเงื่อนไขการส่งเควสแบบใหม่
        if (currentQuest.questType == QuestType.TalkOnly)
        {
            // ถ้าเป็นเควสแบบเดินมาคุย ก็ให้ผ่านได้เลย
            canComplete = true;
        }
        else
        {
            // ถ้าเป็นเควสหาของ เช็คกระเป๋าว่ามีของครบไหม
            if (Inventory.instance != null && Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount))
            {
                canComplete = true;
            }
        }

        // ถ้าเงื่อนไขผ่าน ให้จบเควส
        if (canComplete)
        {
            CompleteCurrentQuest(currentQuest);
        }
        else
        {
            // ถ้าของไม่ครบ หรือยังไม่ผ่าน ให้พูดประโยคให้ไปหาของซ้ำ
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(currentQuest.questStartDialogue, false));
        }
    }

    void CompleteCurrentQuest(QuestData quest)
    {
        quest.isCompleted = true;

        // หักไอเทมในกระเป๋า (เฉพาะประเภท Consumable)
        if (quest.questType == QuestType.Consumable)
        {
            if (Inventory.instance != null)
                Inventory.instance.RemoveItem(quest.questItem, quest.requiredAmount);
        }

        // ⭐ [อัปเกรด] แจกของรางวัลเข้ากระเป๋าผู้เล่น!
        if (quest.rewardItem != null && Inventory.instance != null)
        {
            for (int i = 0; i < quest.rewardAmount; i++)
            {
                // สมมติว่าระบบกระเป๋าใช้คำสั่ง AddItem (ถ้าของเดิมเป็นชื่ออื่น แจ้งผมแก้ให้ได้นะครับ)
                Inventory.instance.AddItem(quest.rewardItem);
            }
        }

        if (BookUI.instance != null)
            BookUI.instance.UnlockNewPage(quest.pageToUnlock);

        // ⭐ [อัปเกรด] สั่งทำงาน Event "ตอนส่งเควสสำเร็จ" ทันที!
        quest.onQuestCompleted?.Invoke();

        currentQuestIndex++;

        if (currentQuestIndex >= quests.Count && allQuestsDoneUI != null)
            allQuestsDoneUI.SetActive(true);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogueCoroutine(quest.questSuccessDialogue, false));
    }
}