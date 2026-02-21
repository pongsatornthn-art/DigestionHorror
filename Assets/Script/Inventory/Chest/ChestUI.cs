using UnityEngine;
using UnityEngine.UI;

public class ChestUI : MonoBehaviour
{
    public static ChestUI instance;

    [Header("UI References")]
    public GameObject chestPanel;
    public Transform itemsParent;

    private InventorySlot[] slots;
    private LootBox currentBox;

    // ⭐ เพิ่มตัวแปรเก็บเวลา เพื่อป้องกันบัคกด E เปิดปุ๊บ ปิดปั๊บ
    private float openTime;

    void Awake()
    {
        instance = this;
        slots = itemsParent.GetComponentsInChildren<InventorySlot>(true);
    }

    void Start()
    {
        if (chestPanel != null)
            chestPanel.SetActive(false);
    }

    // ⭐ Update ของ ChestUI มีหน้าที่แค่ "รอรับคำสั่งกดปิด" เท่านั้นครับ
    void Update()
    {
        // ถ้าหน้าต่างกล่องเปิดอยู่
        if (chestPanel.activeSelf)
        {
            // เช็คว่าเปิดมาแล้วเกิน 0.1 วินาทีหรือยัง (กันบัคปุ่ม E ลั่น)
            if (Time.unscaledTime > openTime + 0.1f)
            {
                // ถ้ากด E หรือ ESC ให้ทำการปิด
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseChest();

                    // สั่งให้กระเป๋าผู้เล่นปิดตามไปด้วย
                    if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel.activeSelf)
                    {
                        InventoryUI.instance.ToggleInventory();
                    }
                }

            }
        }
    }

    public void ShowChest(LootBox box)
    {
        currentBox = box;
        chestPanel.SetActive(true);

        // จดจำเวลาที่เปิดกล่อง 
        openTime = Time.unscaledTime;

        UpdateUI();
    }

    public void CloseChest()
    {
        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }
        currentBox = null;
    }

    // ==================================================
    // ฟังก์ชันอัปเดตของในกล่อง (หัวใจหลัก)
    // ==================================================

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
            slots[i].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (currentBox != null && currentBox.boxContents != null)
        {
            for (int i = 0; i < currentBox.boxContents.Count; i++)
            {
                if (i < slots.Length)
                {
                    InventoryItem itemInfo = currentBox.boxContents[i];

                    slots[i].AddItem(itemInfo.itemData, itemInfo.amount, false);

                    int index = i;
                    slots[i].GetComponent<Button>().onClick.AddListener(() => TakeItem(index));
                }
            }
        }
    }

    void TakeItem(int slotIndex)
    {
        if (currentBox != null && slotIndex < currentBox.boxContents.Count)
        {
            InventoryItem itemToTake = currentBox.boxContents[slotIndex];

            bool success = Inventory.instance.AddItem(itemToTake.itemData, itemToTake.amount);

            if (success)
            {
                currentBox.RemoveItem(itemToTake);
                UpdateUI();
                Debug.Log($"หยิบ {itemToTake.itemData.itemName} สำเร็จ!");
            }
            else
            {
                Debug.Log("กระเป๋าเต็ม! หยิบไม่ได้");
            }
        }
    }
}