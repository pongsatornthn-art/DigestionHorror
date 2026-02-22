using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI instance;

    [Header("UI References (ลาก UI มาใส่ให้ตรง)")]
    public GameObject shopPanel;
    public Transform itemsParent;

    [Header("NPC UI (ฝั่งซ้าย)")]
    public Image npcPortraitImage;
    public TextMeshProUGUI npcDialogueText;

    [Header("Player Economy UI (ฝั่งขวาบน)")]
    public TextMeshProUGUI playerMoneyText;

    private ShopSlot[] slots;
    private ShopNPC currentNPC;
    private float openTime;

    void Awake()
    {
        instance = this;
        slots = itemsParent.GetComponentsInChildren<ShopSlot>(true);
    }

    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    void Update()
    {
        if (shopPanel.activeSelf && Time.unscaledTime > openTime + 0.1f)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop(); // เรียกคำสั่งปิดตัวเดียวพอ เดี๋ยวระบบจัดการปิดทุกอย่างให้เอง
            }
        }
    }

    public void OpenShop(ShopNPC npc)
    {
        currentNPC = npc;
        shopPanel.SetActive(true);
        openTime = Time.unscaledTime;

        if (npcPortraitImage != null) npcPortraitImage.sprite = npc.portrait;
        if (npcDialogueText != null) npcDialogueText.text = npc.greetingText;

        UpdateUI();
    }

    // ⭐ ฟังก์ชันปิดร้านค้า (เวอร์ชันสมบูรณ์ ปิดหมดทุกอย่าง)
    public void CloseShop()
    {
        // 1. ปิดหน้าร้านค้า
        if (shopPanel != null) shopPanel.SetActive(false);
        currentNPC = null;

        // 2. ปิดกระเป๋าและคราฟต์
        if (InventoryUI.instance != null)
        {
            if (InventoryUI.instance.inventoryPanel != null)
                InventoryUI.instance.inventoryPanel.SetActive(false);

            if (InventoryUI.instance.craftingPanel != null)
                InventoryUI.instance.craftingPanel.SetActive(false);
        }
    }

    public void UpdateUI()
    {
        if (playerMoneyText != null && PlayerController.instance != null)
        {
            playerMoneyText.text = PlayerController.instance.currentMoney.ToString() + " $";
        }

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
            slots[i].buyButton.onClick.RemoveAllListeners();
        }

        if (currentNPC != null && currentNPC.itemsForSale != null)
        {
            for (int i = 0; i < currentNPC.itemsForSale.Count; i++)
            {
                if (i < slots.Length)
                {
                    ItemData item = currentNPC.itemsForSale[i];
                    slots[i].SetupSlot(item);
                    slots[i].buyButton.onClick.AddListener(() => BuyItem(item));
                }
            }
        }
    }

    void BuyItem(ItemData itemToBuy)
    {
        int itemPrice = itemToBuy.price;

        if (PlayerController.instance.currentMoney >= itemPrice)
        {
            bool success = Inventory.instance.AddItem(itemToBuy, 1);

            if (success)
            {
                PlayerController.instance.currentMoney -= itemPrice;
                UpdateUI();
                PlayerController.instance.SendMessage("UpdateUI");

                if (npcDialogueText != null) npcDialogueText.text = "ขอบคุณที่อุดหนุนนะ!";
                Debug.Log($"🛒 ซื้อ {itemToBuy.itemName} สำเร็จ!");
            }
            else
            {
                if (npcDialogueText != null) npcDialogueText.text = "กระเป๋าของนายเต็มแล้วนะ!";
            }
        }
        else
        {
            if (npcDialogueText != null) npcDialogueText.text = "เงินไม่พอนะไอ้หนุ่ม!";
        }
    }
}