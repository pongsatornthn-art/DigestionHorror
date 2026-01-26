using UnityEngine;
using UnityEngine.UI;

public class ChestUI : MonoBehaviour
{
    public static ChestUI instance; // Singleton ให้คนอื่นเรียกใช้ได้

    [Header("UI References")]
    public GameObject chestPanel;   // ตัวหน้าต่างกล่อง (ChestPanel)
    public Transform itemsParent;   // ตัว Grid ที่ใส่ Slot (ChestGrid)

    private InventorySlot[] slots;  // อาเรย์เก็บช่องเก็บของทั้งหมด
    private LootBox currentBox;     // จำไว้ว่าเปิดกล่องใบไหนอยู่

    void Awake()
    {
        instance = this;
        // ดึง Slot ทั้งหมดที่อยู่ใน Grid มาเตรียมไว้
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
    }

    void Start()
    {
        // เริ่มเกมมา ซ่อนหน้าต่างกล่องก่อน
        if (chestPanel != null)
            chestPanel.SetActive(false);
    }

    void Update()
    {
        // ถ้าหน้าต่างเปิดอยู่ แล้วกด ESC ให้ปิด
        if (chestPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChest();
        }
    }

    // ==================================================
    // ฟังก์ชันเปิด/ปิด
    // ==================================================

    public void ShowChest(LootBox box)
    {
        currentBox = box;           // จำข้อมูลกล่องที่เปิด
        chestPanel.SetActive(true); // เปิดหน้าต่าง

        UpdateUI();                 // โหลดของมาโชว์
    }

    public void CloseChest()
    {
        chestPanel.SetActive(false); // ปิดหน้าต่างตัวเอง
        currentBox = null;           // ลืมกล่องซะ

        // ⭐ สั่งปิดกระเป๋าผู้เล่นด้วย (เพื่อความเนียน)
        if (InventoryUI.instance != null)
        {
            InventoryUI.instance.inventoryPanel.SetActive(false);
        }
    }

    // ==================================================
    // ฟังก์ชันอัปเดตของในกล่อง (หัวใจหลัก)
    // ==================================================

    public void UpdateUI()
    {
        // 1. เคลียร์ของเก่าในหน้าจอออกให้หมดก่อน
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
            // ลบคำสั่งกดปุ่มเก่าทิ้ง (สำคัญมาก! ไม่งั้นกดช่องเดิมจะ Error)
            slots[i].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        // 2. ถ้ามีกล่อง และมีของข้างใน
        if (currentBox != null && currentBox.boxContents != null)
        {
            for (int i = 0; i < currentBox.boxContents.Count; i++)
            {
                // เช็คว่าช่องพอไหม
                if (i < slots.Length)
                {
                    InventoryItem itemInfo = currentBox.boxContents[i];

                    // ใส่รูปและตัวเลขลงช่อง
                    slots[i].AddItem(itemInfo.itemData, itemInfo.amount, false);

                    // ⭐ สร้างปุ่ม "คลิกเพื่อหยิบ" (Click to Loot)
                    int index = i; // จำลำดับช่องไว้ส่งให้ฟังก์ชัน (ป้องกันค่าเพี้ยน)
                    slots[i].GetComponent<Button>().onClick.AddListener(() => TakeItem(index));
                }
            }
        }
    }

    // ==================================================
    // ฟังก์ชันหยิบของ (ทำงานเมื่อคลิกที่ช่อง)
    // ==================================================

    void TakeItem(int slotIndex)
    {
        if (currentBox != null && slotIndex < currentBox.boxContents.Count)
        {
            InventoryItem itemToTake = currentBox.boxContents[slotIndex];

            // 1. พยายามยัดใส่กระเป๋าผู้เล่น
            bool success = Inventory.instance.AddItem(itemToTake.itemData, itemToTake.amount);

            // 2. ถ้าใส่สำเร็จ (กระเป๋าไม่เต็ม)
            if (success)
            {
                // ลบออกจากกล่อง
                currentBox.RemoveItem(itemToTake);

                // อัปเดตหน้าจอใหม่ (ของหายไปจากกล่อง)
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