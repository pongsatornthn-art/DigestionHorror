using UnityEngine;

public class WeaponRepairHandler : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Weapon[] allWeapons;
    public float repairAmount = 20f;

    [Header("Repair Requirements")]
    public ItemData requiredRepairItem;
    public int requiredAmount = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TryRepairActiveWeapon();
        }
    }

    void TryRepairActiveWeapon()
    {
        foreach (Weapon weapon in allWeapons)
        {
            // เช็คว่าถืออาวุธชิ้นนี้อยู่หรือไม่
            if (weapon != null && weapon.gameObject.activeInHierarchy)
            {
                if (weapon.IsFull())
                {
                    Debug.Log("อาวุธเต็มอยู่ ไม่ต้องซ่อม");
                    return;
                }

                // 1. เช็คของจาก InventoryManager จริงๆ
                if (HasRepairItem(requiredRepairItem, requiredAmount))
                {
                    // 2. สั่งลบไอเทม (เอา // ออกแล้ว)
                    RemoveRepairItem(requiredRepairItem, requiredAmount);

                    // 3. สั่งซ่อม
                    weapon.RepairWeapon(repairAmount);
                    Debug.Log($"ซ่อมสำเร็จ! ใช้ {requiredRepairItem.itemName}");
                }
                else
                {
                    Debug.Log("ไอเทมซ่อมไม่พอ!");
                }
                return;
            }
        }
    }


    bool HasRepairItem(ItemData itemToCheck, int amountNeeded)
    {
        if (itemToCheck == null || Inventory.Instance == null) return false;

        return Inventory.Instance.CheckItem(itemToCheck) >= amountNeeded;
    }

    void RemoveRepairItem(ItemData itemToRemove, int amountToRemove)
    {
        if (itemToRemove == null || Inventory.Instance == null) return;

        Inventory.Instance.RemoveItem(itemToRemove, amountToRemove);

    }
}