using UnityEngine;
using System;

public class Weapon : MonoBehaviour
{
    public ItemData data;
    public float currentDurability;
    public static event Action<float> OnDurabilityChanged;

    public bool IsBroken() => currentDurability <= 0;

    // ⭐ 1. เปลี่ยนจาก Start เป็น Awake เพื่อให้เซ็ตค่าความทนทานเตรียมไว้ตั้งแต่เริ่มเกม
    void Awake()
    {
        if (data != null)
        {
            currentDurability = data.maxDurability;
        }
    }

    // ⭐ 2. เพิ่มฟังก์ชันนี้: จะถูกเรียกอัตโนมัติทุกครั้งที่สลับมาถืออาวุธชิ้นนี้
    void OnEnable()
    {
        UpdateUI();
    }

    public bool IsFull() => currentDurability >= data.maxDurability;

    public void UseWeapon(float amount)
    {
        if (data == null) return;
        currentDurability -= amount;
        currentDurability = Mathf.Clamp(currentDurability, 0, data.maxDurability);
        UpdateUI();
    }

    public void RepairWeapon(float amount)
    {
        if (data == null) return;
        currentDurability += amount;
        currentDurability = Mathf.Clamp(currentDurability, 0, data.maxDurability);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (data != null && data.maxDurability > 0)
        {
            OnDurabilityChanged?.Invoke(currentDurability / data.maxDurability);
        }
    }
}