using System.Collections.Generic;
using UnityEngine;

public class LootBox : MonoBehaviour
{
    [Header("Settings")]
    public LootTable lootTable;
    public List<InventoryItem> boxContents = new List<InventoryItem>();

    [Header("Normal Box Settings")]
    public bool destroyAfterOpen = false;

    [Header("Interaction (ระยะการเปิดกล่อง)")]
    public float interactRange = 2f; // ⭐ ปรับระยะการยืนเปิดกล่องได้ที่ Inspector
    private Transform player;

    private bool isPlayerDeathBox = false;
    private bool hasStartedDestroyTimer = false;

    public void SetBoxContents(List<InventoryItem> droppedItems)
    {
        isPlayerDeathBox = true;
        boxContents = droppedItems;
    }

    void Start()
    {
        // เริ่มเกมปุ๊บ สั่งให้กล่องรายงานตัวเลยว่าหา Player เจอไหม
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("✅ [LootBox] เจอตัว Player แล้ว!");
        }
        else
        {
            Debug.LogError("❌ [LootBox] หาตัว Player ไม่เจอ! (ลืมตั้ง Tag เป็น Player ให้ตัวละครหรือเปล่า?)");
        }

        if (!isPlayerDeathBox && lootTable != null)
        {
            boxContents = lootTable.GenerateLoot();
        }
    }

    void Update()
    {
        // ดักจับเวลากดปุ่ม E ดูก่อนเลย
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (player == null)
            {
                Debug.LogWarning("⚠️ กด E แล้ว แต่ระบบหาตัว Player ไม่เจอ (กลับไปเช็ค Tag ด่วน!)");
                return; // หยุดทำงานทันที
            }

            // คำนวณระยะห่าง แล้วปริ้นท์บอกใน Console
            float distance = Vector2.Distance(transform.position, player.position);
            Debug.Log($"📏 กด E แล้ว! ระยะห่างคือ: {distance} (ระยะที่เปิดได้คือ {interactRange})");

            if (distance <= interactRange)
            {
                bool isChestOpen = ChestUI.instance != null && ChestUI.instance.chestPanel.activeSelf;
                if (!isChestOpen)
                {
                    Debug.Log("🔓 อยู่ในระยะ! กำลังสั่งเปิดกล่อง...");
                    OpenChest();
                }
                else
                {
                    Debug.Log("⚠️ หน้าต่างกล่องเปิดอยู่แล้ว");
                }
            }
            else
            {
                Debug.Log("❌ อยู่ไกลเกินไป เปิดไม่ได้! ต้องเดินเข้าไปใกล้กว่านี้");
            }
        }
    }

    // ❌ เราจะลบ OnMouseEnter กับ OnMouseExit ทิ้งไปเลยครับ ไม่ต้องใช้เมาส์ชี้แล้ว!

    void OpenChest()
    {
        Debug.Log("🔓 สั่งเปิดกล่องและกระเป๋าแล้ว!");

        // 1. สั่งเปิดหน้าต่างกล่อง
        if (ChestUI.instance != null)
        {
            ChestUI.instance.ShowChest(this);
        }

        // ⭐ 2. สั่งบังคับเปิดหน้าต่างกระเป๋าตรงๆ (ห้ามใช้ ToggleInventory() ตรงนี้เด็ดขาด!)
        if (InventoryUI.instance != null)
        {
            InventoryUI.instance.inventoryPanel.SetActive(true);

            // ถ้าอยากให้หน้าต่างคราฟต์เปิดมาพร้อมกล่องด้วย ก็เอา // ข้างหน้าบรรทัดล่างออกครับ
            if (InventoryUI.instance.craftingPanel != null) InventoryUI.instance.craftingPanel.SetActive(true);
        }

        // 3. เริ่มนับเวลาทำลายกล่อง (ถ้าเป็นกล่องคนตาย หรือตั้งค่าให้ลบ)
        if ((isPlayerDeathBox || destroyAfterOpen) && !hasStartedDestroyTimer)
        {
            hasStartedDestroyTimer = true;
            Destroy(gameObject, 10f);
        }
    }

    public void RemoveItem(InventoryItem itemToRemove)
    {
        if (boxContents.Contains(itemToRemove))
        {
            boxContents.Remove(itemToRemove);
        }
    }

    void OnDestroy()
    {
        // เติมเงื่อนไขเช็คว่าหน้าต่าง != null (ยังไม่ถูกลบ) ก่อนเข้าไปสั่งปิด
        if (ChestUI.instance != null && ChestUI.instance.chestPanel != null && ChestUI.instance.chestPanel.activeSelf)
        {
            ChestUI.instance.CloseChest();

            if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel != null && InventoryUI.instance.inventoryPanel.activeSelf)
            {
                InventoryUI.instance.ToggleInventory();
            }
        }
    }

    // ⭐ เพิ่มฟังก์ชันนี้ เพื่อให้มีเส้นวงกลมสีเหลืองวาดบอกระยะรอบๆ กล่อง (ดูได้ในหน้า Scene)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}