using UnityEngine;
using System.Collections;

public class QuestNPC : MonoBehaviour
{
    public enum QuestType { Consumable, NonConsumable }

    [Header("Quest Settings")]
    public QuestType questType;
    public ItemData questItem;
    public int requiredAmount = 1;
    public int pageToUnlock = 1;
    public float interactDistance = 3f;

    [Header("UI Feedback (Images)")]
    public GameObject startQuestUI;
    public GameObject successUI;

    private Transform player;
    private bool hasAcceptedQuest = false;
    private bool isQuestCompleted = false;
    private bool isMouseOver = false;

    void Start()
    {
        // อ้างอิง PlayerController
        if (PlayerController.instance != null)
            player = PlayerController.instance.transform;

        if (startQuestUI != null) startQuestUI.SetActive(false);
        if (successUI != null) successUI.SetActive(false);
    }

    // ✅ ต้องเอาเมาส์ชี้ที่ตัว NPC เท่านั้น
    void OnMouseEnter() { isMouseOver = true; }
    void OnMouseExit() { isMouseOver = false; }

    void Update()
    {
        if (isQuestCompleted || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // ✅ เงื่อนไข: อยู่ใกล้ + เมาส์ชี้ที่ตัว NPC + กด E
        if (dist <= interactDistance && isMouseOver && Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        if (!hasAcceptedQuest)
        {
            hasAcceptedQuest = true;
            StartCoroutine(ShowUI(startQuestUI));
            return;
        }

        // เช็คไอเทมใน Inventory
        if (hasAcceptedQuest && Inventory.instance.HasItem(questItem, requiredAmount))
        {
            CompleteQuest();
        }
        else
        {
            StartCoroutine(ShowUI(startQuestUI));
        }
    }

    void CompleteQuest()
    {
        isQuestCompleted = true;
        if (questType == QuestType.Consumable)
        {
            Inventory.instance.RemoveItem(questItem, requiredAmount);
        }

        // ปลดล็อกหน้าสมุด
        if (BookUI.instance != null)
            BookUI.instance.UnlockNewPage(pageToUnlock);

        StartCoroutine(ShowUI(successUI));
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