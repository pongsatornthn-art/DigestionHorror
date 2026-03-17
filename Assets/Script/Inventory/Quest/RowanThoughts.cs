using UnityEngine;
using TMPro;
using System.Collections;

public class RowanThoughts : MonoBehaviour
{
    public GameObject thoughtPanel; // กล่องข้อความดำๆ (สร้างแยกไว้ที่ UI)
    public TMP_Text thoughtText;    // ตัวหนังสือ
    [TextArea] public string thoughtMessage; // ข้อความบ่นในใจ
    public float displayTime = 3f;  // เวลาที่จะให้โชว์ค้างไว้บนจอ

    private bool hasTriggered = false; // กันไม่ให้ข้อความเด้งซ้ำๆ

    void Start()
    {
        if (thoughtPanel != null) thoughtPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ถ้า Player เดินมาเหยียบ และยังไม่เคยพูดข้อความนี้
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ShowThought());
        }
    }

    IEnumerator ShowThought()
    {
        if (thoughtPanel != null && thoughtText != null)
        {
            thoughtText.text = thoughtMessage;
            thoughtPanel.SetActive(true);

            // รอเวลาตามที่ตั้งไว้
            yield return new WaitForSeconds(displayTime);

            thoughtPanel.SetActive(false);
        }
    }
}