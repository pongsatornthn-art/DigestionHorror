using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class RowanThoughts : MonoBehaviour
{
    public GameObject thoughtPanel;
    public TMP_Text thoughtText;
    [TextArea] public string thoughtMessage;
    public float displayTime = 3f;

    [Header("ตั้งค่าความสมูทของเนื้อเรื่อง")]
    public float delayBeforeShow = 0f;

    [Header("เงื่อนไขไอเทม (ไม่ใส่ก็ได้)")]
    public ItemData requiredItem;
    public int requiredAmount = 1;

    [Header("ป้องกันการข้ามเนื้อเรื่อง (ล็อกเควส)")]
    public DualNPC questNPC;
    [Tooltip("ใส่ลำดับเควสที่ต้องการให้ข้อความนี้ทำงาน (Quest 1 ใส่ 0) / ถ้าใส่ -1 คือปล่อยผ่าน")]
    public int requiredQuestIndex = -1;

    // ⭐ เพิ่มฟีเจอร์ใหม่: เล่นก่อนรับเควส
    [Tooltip("ถ้าติ๊กถูก: ข้อความจะโชว์ 'ก่อน' ที่จะกดรับเควสนี้ (พอรับเควสแล้วจะไม่พูดอีก)")]
    public bool playBeforeQuest = false;

    [Header("🔥 โหมดเผชิญหน้าบอส (Boss Phase)")]
    [Tooltip("ถ้าติ๊กถูก ข้อความนี้จะเด้งก็ต่อเมื่อ ส่งเควสครบทุกอัน และบอสปรากฏตัวแล้วเท่านั้น!")]
    public bool requireBossActive = false;

    [Header("เหตุการณ์ต่อเนื่อง (เมื่อพูดจบ)")]
    public UnityEvent onThoughtFinished;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            CheckAndPlayThought();
        }
    }

    public void PlayThoughtNow()
    {
        if (!hasTriggered)
        {
            CheckAndPlayThought();
        }
    }

    private void CheckAndPlayThought()
    {
        // 1. โหมดบอส
        if (requireBossActive && questNPC != null)
        {
            if (questNPC.currentQuestIndex < questNPC.quests.Count)
            {
                return;
            }
        }
        // 2. โหมดอิงตามเควส
        else if (questNPC != null && requiredQuestIndex >= 0)
        {
            if (playBeforeQuest)
            {
                // ⭐ โหมดใหม่: ให้พูด "ก่อน" รับเควส
                // ถ้าเลยเควสนั้นไปแล้ว หรือ รับเควสนั้นไปแล้ว -> ให้เงียบ
                if (questNPC.currentQuestIndex > requiredQuestIndex ||
                   (questNPC.currentQuestIndex == requiredQuestIndex && questNPC.quests[requiredQuestIndex].hasAccepted))
                {
                    return;
                }
            }
            else
            {
                // โหมดเดิม: ให้พูด "ระหว่าง" ทำเควส
                if (questNPC.currentQuestIndex != requiredQuestIndex ||
                    !questNPC.quests[requiredQuestIndex].hasAccepted)
                {
                    return;
                }
            }
        }

        // 3. เช็คระบบไอเทม
        if (requiredItem != null)
        {
            if (Inventory.instance == null || !Inventory.instance.HasItem(requiredItem, requiredAmount))
            {
                return;
            }
        }

        // ลุยโชว์ข้อความ!
        hasTriggered = true;
        StartCoroutine(ShowThoughtCoroutine());
    }

    IEnumerator ShowThoughtCoroutine()
    {
        if (delayBeforeShow > 0)
            yield return new WaitForSeconds(delayBeforeShow);

        if (thoughtPanel != null) thoughtPanel.SetActive(true);
        if (thoughtText != null) thoughtText.text = thoughtMessage;

        yield return new WaitForSeconds(displayTime);

        if (thoughtPanel != null) thoughtPanel.SetActive(false);

        onThoughtFinished?.Invoke();
    }
}