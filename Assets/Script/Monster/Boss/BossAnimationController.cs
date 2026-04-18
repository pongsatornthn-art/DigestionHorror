using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [Header("Hitbox References")]
    [SerializeField] private GameObject leftHandHitbox;
    [SerializeField] private GameObject rightHandHitbox;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip leftHandSwingSound;  // เสียงแขนซ้าย (Attack 01)
    [SerializeField] private AudioClip rightHandSwingSound; // เสียงแขนขวา (Attack 02)
    [SerializeField] private AudioClip bothHandSwingSound;  // ⭐ เสียงทุบ 2 มือ (Attack 03)

    // ฟังก์ชันสำหรับเล่นเสียง (เรียกจาก Animation Event)
    public void PlaySwingSound(string hand)
    {
        // 🛡️ เพิ่มการเช็คความปลอดภัย
        if (audioSource == null || this == null) return;

        if (hand == "Left" && leftHandSwingSound != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(leftHandSwingSound);
        }
        else if (hand == "Right" && rightHandSwingSound != null)
        {
            audioSource.pitch = 0.85f;
            audioSource.PlayOneShot(rightHandSwingSound);
            Debug.Log("Right Hand Smash!");
        }
        else if (hand == "Both" && bothHandSwingSound != null)
        {
            // ⭐ ท่าที่ 3: ปรับเสียงให้ทุ้มและหนักขึ้น
            audioSource.pitch = 0.75f;
            audioSource.PlayOneShot(bothHandSwingSound);
            Debug.Log("💥 DOUBLE SMASH!");
        }
    }

    public void EnableDamage(string hand)
    {
        Debug.Log("Animation Event: EnableDamage called with hand: " + hand);

        if (hand == "Left" && leftHandHitbox != null)
        {
            leftHandHitbox.SetActive(true);
        }
        else if (hand == "Right" && rightHandHitbox != null)
        {
            rightHandHitbox.SetActive(true);
        }
        else if (hand == "Both")
        {
            // ⭐ ท่าที่ 3: เปิด Hitbox ดาเมจทั้ง 2 ข้างพร้อมกันเลย!
            if (leftHandHitbox != null) leftHandHitbox.SetActive(true);
            if (rightHandHitbox != null) rightHandHitbox.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Hand name not matched or Hitbox is missing in Inspector!");
        }
    }

    public void DisableDamage()
    {
        Debug.Log("Animation Event: DisableDamage called");
        if (leftHandHitbox != null) leftHandHitbox.SetActive(false);
        if (rightHandHitbox != null) rightHandHitbox.SetActive(false);
    }
}