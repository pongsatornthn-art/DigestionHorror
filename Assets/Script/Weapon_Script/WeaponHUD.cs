using UnityEngine;
using UnityEngine.UI;

public class WeaponHUD : MonoBehaviour
{
    public Slider durabilitySlider; // เปลี่ยนจาก Image เป็น Slider

    private void OnEnable()
    {
        Weapon.OnDurabilityChanged += UpdateDurabilityBar;
    }

    private void OnDisable()
    {
        Weapon.OnDurabilityChanged -= UpdateDurabilityBar;
    }

    void UpdateDurabilityBar(float fillAmount)
    {
        if (durabilitySlider != null)
        {
            // Slider ปกติจะมีค่าตั้งแต่ 0 ถึง 1 อยู่แล้ว (ถ้าไม่ได้ไปแก้ Max Value)
            durabilitySlider.value = fillAmount;
        }
    }
}