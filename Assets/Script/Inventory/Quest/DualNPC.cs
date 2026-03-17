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

        [TextArea] public string questStartDialogue = "เจ้าช่วยหาของสิ่งนี้มาให้ข้าหน่อยได้ไหม?";
        [TextArea] public string questSuccessDialogue = "โอ้! ขอบใจเจ้ามาก นี่คือรางวัลของเจ้า!";

        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool hasAccepted = false;
    }

    [Header("NPC Basic Info")]
    public string npcName = "Mysterious Merchant";
    public Sprite portrait;
    [TextArea] public string greetingText = "Welcome!";

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
    private bool hasGreeted = false;

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

        if (!hasGreeted)
        {
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(greetingText, true));
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeDialogueCoroutine(greetingText, true, true));
        }
    }

    IEnumerator TypeDialogueCoroutine(string textToType, bool isGreeting, bool skipTyping = false)
    {
        string fullText = npcName + " : " + textToType;

        yield return new WaitForSeconds(0.1f); // กันบั๊กปุ่มลั่น

        if (dialogueText != null)
        {
            if (skipTyping)
            {
                dialogueText.text = fullText;
            }
            else
            {
                dialogueText.text = npcName + " : ";

                foreach (char letter in textToType.ToCharArray())
                {
                    dialogueText.text += letter;

                    // ⭐ ตัดเมาส์ออก เหลือแค่กด E หรือ Spacebar เพื่อเร่งตัวหนังสือ
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                    {
                        dialogueText.text = fullText;
                        yield return new WaitForSeconds(0.2f);
                        break;
                    }

                    yield return new WaitForSeconds(typingSpeed);
                }
            }
        }

        // ⭐ ตัดเมาส์ออก รอจนกว่าผู้เล่นจะกด E หรือ Space เพื่อไปต่อ!
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space));

        yield return new WaitForSeconds(0.1f);

        if (isGreeting)
        {
            hasGreeted = true;
            OpenSelectionPanel();
        }
        else
        {
            CloseEverything();
        }
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