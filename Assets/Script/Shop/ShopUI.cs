using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI instance;

    [Header("UI References")]
    public GameObject shopPanel;
    public Transform itemsParent;

    [Header("NPC UI (Left Side)")]
    public Image npcPortraitImage;
    public TextMeshProUGUI npcDialogueText;

    [Header("Player Economy UI (Top Right)")]
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
                CloseShop();
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

    // ⭐ Close Shop and other panels
    public void CloseShop()
    {
        // 1. Close Shop UI
        if (shopPanel != null) shopPanel.SetActive(false);
        currentNPC = null;

        // 2. Close Inventory and Crafting UI
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

                if (npcDialogueText != null) npcDialogueText.text = "Thanks for your purchase!";
                Debug.Log($"🛒 Successfully bought {itemToBuy.itemName}!");
            }
            else
            {
                if (npcDialogueText != null) npcDialogueText.text = "Your inventory is full!";
            }
        }
        else
        {
            if (npcDialogueText != null) npcDialogueText.text = "You don't have enough money for that, kid.";
        }
    }
}