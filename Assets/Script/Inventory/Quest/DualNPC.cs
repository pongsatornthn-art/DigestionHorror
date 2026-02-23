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
        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool hasAccepted = false;
    }

    [Header("NPC Basic Info")]
    public string npcName = "Mysterious Merchant";
    public Sprite portrait;
    [TextArea] public string greetingText = "Welcome! How can I help you today?";

    [Header("Shop Settings")]
    public List<ItemData> itemsForSale;

    [Header("Quest Settings")]
    public List<QuestData> quests = new List<QuestData>();
    private int currentQuestIndex = 0;

    [Header("UI Selection Panel")]
    public GameObject selectionPanel; // ลาก NPCSelectionPanel มาใส่
    public Button shopButton;         // ลากปุ่ม Shop มาใส่
    public Button questButton;        // ลากปุ่ม Quest มาใส่
    public Button closeButton;        // ลากปุ่ม Close (ถ้ามี) มาใส่

    [Header("General Settings")]
    public float interactDistance = 3f;
    public GameObject allQuestsDoneUI;

    private Transform player;
    private bool isMouseOver = false;
    private bool isShowingUI = false;

    void Start()
    {
        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;

        // ล้างค่า UI เควส
        foreach (var q in quests)
        {
            if (q.startQuestUI != null) q.startQuestUI.SetActive(false);
            if (q.successUI != null) q.successUI.SetActive(false);
        }

        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (allQuestsDoneUI != null) allQuestsDoneUI.SetActive(false);

        // ⭐ ล้าง Event เก่าออกก่อนเพื่อกันบั๊กกดครั้งเดียวทำงานสองรอบ
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

    void OnMouseEnter() { isMouseOver = true; }
    void OnMouseExit() { isMouseOver = false; }

    void Update()
    {
        if (player == null || isShowingUI) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= interactDistance && isMouseOver && Input.GetKeyDown(KeyCode.E))
        {
            // เมื่อกด E ให้เปิดหน้าเลือกก่อน
            OpenSelectionPanel();
        }
    }

    // --- ระบบ Selection UI ---
    void OpenSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            isShowingUI = true;

            // ปิดปุ่มเควสถ้าทำครบหมดแล้ว
            if (questButton != null)
                questButton.interactable = currentQuestIndex < quests.Count;
        }
    }

    public void CloseSelectionPanel()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        isShowingUI = false;
    }

    // --- ระบบ Shop (ดึงจาก ShopNPC เดิม) ---
    void OpenShopLogic()
    {
        CloseSelectionPanel();

        if (ShopUI.instance != null)
        {
            // สร้าง ShopNPC ชั่วคราวเพื่อให้ ShopUI อ่านค่าได้ หรือปรับ ShopUI ให้รับค่าจาก DualNPC
            ShopUI.instance.OpenShop(this.gameObject.GetComponent<ShopNPC_Helper>());
        }

        // เปิดกระเป๋าอัตโนมัติ
        if (InventoryUI.instance != null)
        {
            if (InventoryUI.instance.inventoryPanel != null)
                InventoryUI.instance.inventoryPanel.SetActive(true);
            if (InventoryUI.instance.craftingPanel != null)
                InventoryUI.instance.craftingPanel.SetActive(false);
        }
    }

    // --- ระบบ Quest (ดึงจาก QuestNPC เดิม) ---
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

        if (Inventory.instance.HasItem(currentQuest.questItem, currentQuest.requiredAmount))
        {
            CompleteCurrentQuest(currentQuest);
        }
        else
        {
            StartCoroutine(ShowQuestUI(currentQuest.startQuestUI));
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

        StartCoroutine(ShowQuestUI(quest.successUI));
        currentQuestIndex++;

        if (currentQuestIndex >= quests.Count && allQuestsDoneUI != null)
        {
            allQuestsDoneUI.SetActive(true);
        }
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
