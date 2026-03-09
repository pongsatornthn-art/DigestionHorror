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
    public float interactRange = 2f;
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
        FindPlayer();

        if (!isPlayerDeathBox && lootTable != null)
        {
            boxContents = lootTable.GenerateLoot();
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (player == null)
            {
                FindPlayer();
                if (player == null) return;
            }

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= interactRange)
            {
                // ตรวจสอบก่อนว่าเปิดอยู่ไหม
                bool isChestOpen = ChestUI.instance != null && ChestUI.instance.chestPanel.activeSelf;
                if (!isChestOpen)
                {
                    OpenChest();
                }
            }
        }
    }

    // ⭐ เพิ่มฟังก์ชันนี้: งัด UI ขึ้นมาแม้จะโดนปิดซ่อนอยู่!
    void WakeUpUI()
    {
        if (ChestUI.instance == null)
        {
            // ค้นหาแบบทะลุมิติ (หาเจอแม้ GameObject จะโดนติ๊กปิดไว้)
            ChestUI[] foundChests = Resources.FindObjectsOfTypeAll<ChestUI>();
            if (foundChests.Length > 0)
            {
                ChestUI.instance = foundChests[0];
                ChestUI.instance.gameObject.SetActive(true); // บังคับติ๊กถูกเปิดตัวแม่
            }
        }

        if (InventoryUI.instance == null)
        {
            InventoryUI[] foundInvs = Resources.FindObjectsOfTypeAll<InventoryUI>();
            if (foundInvs.Length > 0)
            {
                InventoryUI.instance = foundInvs[0];
                InventoryUI.instance.gameObject.SetActive(true); // บังคับติ๊กถูกเปิดตัวแม่
            }
        }
    }

    void OpenChest()
    {
        // 1. เรียกใช้ระบบปลุก UI ก่อนเลย
        WakeUpUI();

        // 2. สั่งเปิดหน้าต่างกล่อง
        if (ChestUI.instance != null)
        {
            ChestUI.instance.ShowChest(this);
        }
        else
        {
            Debug.LogError("❌ หา ChestUI ไม่เจอในโปรเจกต์เลย!");
            return;
        }

        // 3. สั่งบังคับเปิดหน้าต่างกระเป๋า และ "บังคับปิด" หน้าต่างคราฟต์!
        if (InventoryUI.instance != null)
        {
            // เปิดกระเป๋า
            if (InventoryUI.instance.inventoryPanel != null)
            {
                InventoryUI.instance.inventoryPanel.SetActive(true);
            }

            // ⭐ บังคับปิดหน้าคราฟต์เด็ดขาด! (เพิ่ม SetActive เป็น false)
            if (InventoryUI.instance.craftingPanel != null)
            {
                InventoryUI.instance.craftingPanel.SetActive(false);
            }
        }

        // 4. เริ่มนับเวลาทำลายกล่อง
        if ((isPlayerDeathBox || destroyAfterOpen) && !hasStartedDestroyTimer)
        {
            hasStartedDestroyTimer = true;
            Destroy(gameObject, 10f);
        }
    }

    void OnDestroy()
    {
        if (ChestUI.instance != null && ChestUI.instance.chestPanel != null && ChestUI.instance.chestPanel.activeSelf)
        {
            ChestUI.instance.CloseChest();

            if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel != null && InventoryUI.instance.inventoryPanel.activeSelf)
            {
                InventoryUI.instance.ToggleInventory();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    public void RemoveItem(InventoryItem itemToRemove)
    {
        if (boxContents.Contains(itemToRemove))
        {
            boxContents.Remove(itemToRemove);
        }
    }
}