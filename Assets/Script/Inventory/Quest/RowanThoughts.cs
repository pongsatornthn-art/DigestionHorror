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
    public DualNPC questNPC; // ⭐ ลากตัว NPC พ่อค้ามาใส่ตรงนี้
    [Tooltip("ใส่ลำดับเควสที่ต้องการให้ข้อความนี้ทำงาน (Quest 1 ใส่ 0, Quest 2 ใส่ 1)")]
    public int requiredQuestIndex = -1; // ⭐ ถ้าเป็น -1 คือไม่เช็คเควส ปล่อยผ่านเลย

    [Header("เหตุการณ์ต่อเนื่อง (เมื่อพูดจบ)")]
    public UnityEvent onThoughtFinished;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // ⭐ 1. เช็คระบบเควส (ป้องกันการข้ามเนื้อเรื่อง)
            if (questNPC != null && requiredQuestIndex >= 0)
            {
                // ถ้าคิวเควสยังไม่ถึง หรือ "ยังไม่ได้กดคุยรับเควสนั้น" จาก NPC -> ให้เงียบไว้!
                if (questNPC.currentQuestIndex != requiredQuestIndex ||
                    !questNPC.quests[requiredQuestIndex].hasAccepted)
                {
                    return; // ปล่อยให้ผู้เล่นเดินผ่านไปแบบงงๆ (ไม่พูด)
                }
            }

            // ⭐ 2. เช็คระบบไอเทม
            if (requiredItem != null)
            {
                if (Inventory.instance == null || !Inventory.instance.HasItem(requiredItem, requiredAmount))
                {
                    return;
                }
            }

            // ผ่านทุกเงื่อนไข (ถึงคิวเควสแล้ว + ของครบแล้ว) ลุยเลย!
            hasTriggered = true;
            StartCoroutine(ShowThoughtCoroutine());
        }
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