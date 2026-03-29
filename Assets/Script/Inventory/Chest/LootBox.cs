using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LootBox : MonoBehaviour
{
    [Header("Settings")]
    public LootTable lootTable;
    public List<InventoryItem> boxContents = new List<InventoryItem>();

    [Header("Normal Box Settings")]
    public bool destroyAfterOpen = false;

    [Header("Interaction (ระยะการเปิดกล่อง)")]
    public float interactRange = 2f;

    [Header("Visual & UI (ภาพและปุ่ม)")]
    public GameObject normalObject;
    public GameObject hoverObject;
    public GameObject promptUI;

    // ⭐ ส่วนที่เพิ่มใหม่: ระยะเยื้องจากปลายเมาส์ (ปรับได้ใน Unity)
    public Vector3 promptOffset = new Vector3(0.5f, 0.5f, 0f);

    private Transform player;
    private bool isPlayerDeathBox = false;
    private bool hasStartedDestroyTimer = false;

    private bool isMouseOver = false;
    private Collider2D col;

    public void SetBoxContents(List<InventoryItem> droppedItems)
    {
        isPlayerDeathBox = true;
        boxContents = droppedItems;
    }

    void Start()
    {
        FindPlayer();
        col = GetComponent<Collider2D>();

        if (!isPlayerDeathBox && lootTable != null)
        {
            boxContents = lootTable.GenerateLoot();
        }

        UpdateVisuals(false);
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // ล็อกแกน Z

        bool isHoveringNow = col.OverlapPoint(mouseWorldPos);

        if (isHoveringNow != isMouseOver)
        {
            isMouseOver = isHoveringNow;
            UpdateVisuals(isMouseOver);
        }

        // ⭐ ส่วนที่เพิ่มใหม่: บังคับให้ปุ่ม E วิ่งตามเมาส์ตลอดเวลาที่ชี้อยู่
        if (isMouseOver && promptUI != null)
        {
            promptUI.transform.position = mouseWorldPos + promptOffset;
        }

        if (isMouseOver && Input.GetKeyDown(KeyCode.E))
        {
            if (player == null) FindPlayer();
            if (player == null) return;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= interactRange)
            {
                bool isChestOpen = ChestUI.instance != null && ChestUI.instance.chestPanel.activeSelf;
                if (!isChestOpen)
                {
                    OpenChest();
                }
            }
        }
    }

    void UpdateVisuals(bool isHovering)
    {
        if (normalObject != null) normalObject.SetActive(!isHovering);
        if (hoverObject != null) hoverObject.SetActive(isHovering);
        if (promptUI != null) promptUI.SetActive(isHovering);
    }

    void WakeUpUI()
    {
        if (ChestUI.instance == null)
        {
            ChestUI[] foundChests = Resources.FindObjectsOfTypeAll<ChestUI>();
            if (foundChests.Length > 0) { ChestUI.instance = foundChests[0]; ChestUI.instance.gameObject.SetActive(true); }
        }
        if (InventoryUI.instance == null)
        {
            InventoryUI[] foundInvs = Resources.FindObjectsOfTypeAll<InventoryUI>();
            if (foundInvs.Length > 0) { InventoryUI.instance = foundInvs[0]; InventoryUI.instance.gameObject.SetActive(true); }
        }
    }

    void OpenChest()
    {
        WakeUpUI();

        if (ChestUI.instance != null) ChestUI.instance.ShowChest(this);
        else return;

        if (InventoryUI.instance != null)
        {
            if (InventoryUI.instance.inventoryPanel != null) InventoryUI.instance.inventoryPanel.SetActive(true);
            if (InventoryUI.instance.craftingPanel != null) InventoryUI.instance.craftingPanel.SetActive(false);
        }

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
        if (boxContents.Contains(itemToRemove)) boxContents.Remove(itemToRemove);
    }
}