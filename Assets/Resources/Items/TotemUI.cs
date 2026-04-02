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

       
        if (isActive)
        {
            float timeLeft = DigestionSystem.instance.GetTotemTimeLeft();

            
            int totalSeconds = Mathf.CeilToInt(timeLeft);

            if (timeText != null)
            {
             
                timeText.text = totalSeconds.ToString() + "s"; 
                
           
            }
        }
    }
}