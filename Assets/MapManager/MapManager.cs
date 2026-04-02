using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("UI References")]
    public GameObject mapPanel; // หน้าต่าง UI แผนที่หลัก

    [Header("Quest System Reference")]
    public DualNPC mainNPC; // ลาก NPC พ่อค้า (ที่แจกเควส) มาใส่ตรงนี้

    [Header("Quest Markers (จุดวงกลมเป้าหมาย)")]
    [Tooltip("ใส่รูปวงกลมเรียงตามลำดับเควส (เช่น เควสแรกใส่ช่อง 0, เควสหาไวน์ใส่ช่อง 1)")]
    public GameObject[] questMarkers;

    private bool isMapOpen = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // เริ่มเกมมาให้ซ่อนแผนที่ และซ่อนวงกลมทั้งหมด
        if (mapPanel != null) mapPanel.SetActive(false);
        HideAllMarkers();
    }

    void Update()
    {
        // กดปุ่ม M เพื่อเปิด/ปิดแผนที่
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
    }

    public void ToggleMap()
    {
        // สลับสถานะ เปิด <-> ปิด
        isMapOpen = !isMapOpen;
        mapPanel.SetActive(isMapOpen);

        if (isMapOpen)
        {
            // ถ้ากำลังเปิดแผนที่ ให้คำนวณว่าต้องโชว์วงกลมอันไหน
            UpdateMapMarkers();
        }
    }

    void HideAllMarkers()
    {
        // สั่งปิดรูปวงกลมทุกอัน
        foreach (GameObject marker in questMarkers)
        {
            if (marker != null) marker.SetActive(false);
        }
    }

    public void UpdateMapMarkers()
    {
        if (mainNPC == null) return;

        HideAllMarkers(); // ซ่อนวงกลมอันเก่าก่อน

        // ดึงเลขเควสปัจจุบันมาจาก NPC (0 คือเควสแรก, 1 คือเควสสอง...)
        int currentQuest = mainNPC.currentQuestIndex;

        // เช็คว่าเควสยังไม่หมดใช่ไหม
        if (currentQuest < mainNPC.quests.Count)
        {
            // เช็คว่า "ผู้เล่นกดรับเควสนี้มาหรือยัง?"
            bool isAccepted = mainNPC.quests[currentQuest].hasAccepted;

            // ถ้ากดรับเควสมาแล้ว ให้โชว์วงกลมแดงที่จุดนั้น
            if (isAccepted && currentQuest < questMarkers.Length)
            {
                if (questMarkers[currentQuest] != null)
                {
                    questMarkers[currentQuest].SetActive(true);
                }
            }
        }
    }
}