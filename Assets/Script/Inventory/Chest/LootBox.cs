using System.Collections.Generic;
using UnityEngine;

public class LootBox : MonoBehaviour
{
    [Header("Settings")]
    public LootTable lootTable; // ลากไฟล์ LootTable มาใส่ตรงนี้

    // รายการของที่มีในกล่องตอนนี้
    public List<InventoryItem> boxContents = new List<InventoryItem>();

    private bool isOpened = false;
    private bool isMouseOver = false;

    void Start()
    {
        // เริ่มเกมมา สุ่มของใส่กล่องรอไว้เลย!
        if (lootTable != null)
        {
            boxContents = lootTable.GenerateLoot();
        }
    }

    void Update()
    {
        // เช็คการกดปุ่ม E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // ⭐ เงื่อนไขที่ 1: ถ้าเปิดอยู่แล้ว ให้ปิด (ไม่ต้องสนว่าเมาส์ชี้ไหม)
            if (isOpened)
            {
                CloseChest();
            }
            // ⭐ เงื่อนไขที่ 2: ถ้ายังไม่เปิด แต่เมาส์ชี้อยู่ ให้เปิด
            else if (isMouseOver)
            {
                OpenChest();
            }
        }
    }

    // ฟังก์ชันตรวจจับเมาส์
    void OnMouseEnter() => isMouseOver = true;
    void OnMouseExit() => isMouseOver = false;

    void OpenChest()
    {
        Debug.Log("Open Chest!");
        isOpened = true; // จำสถานะว่าเปิดแล้ว

        // 1. เปิดหน้าจอกระเป๋าเรา (Inventory)
        if (InventoryUI.instance != null)
            InventoryUI.instance.inventoryPanel.SetActive(true);

        // 2. เปิดหน้าจอกล่อง (Chest UI)
        if (ChestUI.instance != null)
            ChestUI.instance.ShowChest(this);
    }

    // ⭐ เพิ่มฟังก์ชันปิดกล่อง
    public void CloseChest()
    {
        Debug.Log("Close Chest!");
        isOpened = false; // รีเซ็ตสถานะเป็นปิด

        // สั่งให้ UI ปิดหน้าต่าง
        if (ChestUI.instance != null)
        {
            ChestUI.instance.CloseChest();
        }
    }

    // ฟังก์ชันให้ UI เรียกใช้ตอนหยิบของออก
    public void RemoveItem(InventoryItem itemToRemove)
    {
        if (boxContents.Contains(itemToRemove))
        {
            boxContents.Remove(itemToRemove);
        }
    }
}