using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class TotemUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject totemPanel; 
    public TextMeshProUGUI timeText; 
    public GameObject totemBG; 

    void Update()
    {
        if (DigestionSystem.instance == null) return;

        bool isActive = DigestionSystem.instance.isTotemActive;

        if (totemPanel != null && totemPanel.activeSelf != isActive)
        {
            totemPanel.SetActive(isActive);
        }
        
        if (totemBG != null && totemBG.activeSelf != isActive)
        {
            totemBG.SetActive(isActive);
        }
        
        if (timeText != null && timeText.gameObject.activeSelf != isActive)
        {
            timeText.gameObject.SetActive(isActive);
        }

        // ⭐ ส่วนที่แก้ใหม่: คำนวณเป็นวินาทีล้วนๆ
        if (isActive)
        {
            float timeLeft = DigestionSystem.instance.GetTotemTimeLeft();

            // ใช้ CeilToInt ปัดเศษทศนิยมขึ้น (สมมติเหลือ 0.5 วิ จะยังโชว์เลข 1 อยู่)
            int totalSeconds = Mathf.CeilToInt(timeLeft);

            if (timeText != null)
            {
                // โชว์เลขวินาที และผมเติมตัว "s" ต่อท้ายให้ดูเป็นหน่วยวินาทีครับ (เช่น 300s)
                timeText.text = totalSeconds.ToString() + "s"; 
                
                // 💡 (ถ้าไม่อยากได้ตัว s ต่อท้าย ให้เปลี่ยนเป็น: timeText.text = totalSeconds.ToString(); ได้เลยครับ)
            }
        }
    }
}