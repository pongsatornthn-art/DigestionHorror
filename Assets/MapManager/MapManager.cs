using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Minimap UI Settings")]
    public GameObject minimapCanvasGroup; // ลากหน้าต่างแผนที่ GPS (minimap) มาใส่ช่องนี้

    [Header("Quest System Reference")]
    public DualNPC mainNPC;

    [Header("Quest Markers (จุดในโลกจริง)")]
    public GameObject[] worldQuestMarkers;

    // ⭐ แก้ตรงนี้เป็น false เพื่อให้เริ่มเกมมาแผนที่ปิดอยู่
    private bool isMapOpen = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // เริ่มเกมมาให้เซ็ตสถานะหน้าต่างตามตัวแปร isMapOpen (ตอนนี้คือจะปิดอยู่)
        if (minimapCanvasGroup != null) minimapCanvasGroup.SetActive(isMapOpen);
        HideAllMarkers();
    }

    void Update()
    {
        // ⭐ นำคำสั่งกด M กลับมาแล้วครับ!
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }

        // อัปเดตจุดเควสให้โชว์ตามสถานะเควสปัจจุบัน (ทำเฉพาะตอนที่แผนที่เปิดอยู่เพื่อลดภาระเครื่อง)
        if (isMapOpen)
        {
            UpdateQuestMarkers();
        }
    }

    // ⭐ ฟังก์ชันสำหรับเปิด/ปิดหน้าต่าง
    public void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        if (minimapCanvasGroup != null) minimapCanvasGroup.SetActive(isMapOpen);
    }

    void HideAllMarkers()
    {
        foreach (GameObject marker in worldQuestMarkers)
        {
            if (marker != null) marker.SetActive(false);
        }
    }

    public void UpdateQuestMarkers()
    {
        if (mainNPC == null) return;

        int currentQuest = mainNPC.currentQuestIndex;

        if (currentQuest < mainNPC.quests.Count)
        {
            bool isAccepted = mainNPC.quests[currentQuest].hasAccepted;

            if (isAccepted && currentQuest < worldQuestMarkers.Length)
            {
                if (worldQuestMarkers[currentQuest] != null && !worldQuestMarkers[currentQuest].activeSelf)
                {
                    HideAllMarkers();
                    worldQuestMarkers[currentQuest].SetActive(true);
                }
            }
        }
        else
        {
            HideAllMarkers();
        }
    }
}