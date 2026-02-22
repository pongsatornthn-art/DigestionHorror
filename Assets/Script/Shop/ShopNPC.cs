using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("NPC Info (ข้อมูลพ่อค้า)")]
    public string npcName = "พ่อค้าลึกลับ";
    public Sprite portrait; // รูปหน้า NPC
    [TextArea] public string greetingText = "ยินดีต้อนรับ! มีอะไรให้รับใช้ไหม?"; // คำทักทาย

    [Header("สินค้าที่ขาย")]
    public List<ItemData> itemsForSale;

    [Header("ระยะการเปิดร้าน")]
    public float interactRange = 2f;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= interactRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                bool isShopOpen = ShopUI.instance != null && ShopUI.instance.shopPanel.activeSelf;
                if (!isShopOpen)
                {
                    OpenShop();
                }
            }
        }
    }

    void OpenShop()
    {
        if (ShopUI.instance != null) ShopUI.instance.OpenShop(this);

        if (InventoryUI.instance != null)
        {
            if (InventoryUI.instance.inventoryPanel != null)
            {
                InventoryUI.instance.inventoryPanel.SetActive(true);
            }
            if (InventoryUI.instance.craftingPanel != null)
            {
                InventoryUI.instance.craftingPanel.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}