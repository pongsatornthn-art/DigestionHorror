using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [Header("Hitbox References")]
    [SerializeField] private GameObject leftHandHitbox;
    [SerializeField] private GameObject rightHandHitbox;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip leftHandSwingSound;  // ใส่เสียงแขนซ้าย
    [SerializeField] private AudioClip rightHandSwingSound; // ใส่เสียงแขนขวา

    // ฟังก์ชันสำหรับเล่นเสียง (เรียกจาก Animation Event)
    // เพิ่มเติมในส่วนฟังก์ชัน PlaySwingSound ใน BossAnimationController.cs
    public void PlaySwingSound(string hand)
    {
        if (audioSource == null) return;

        if (hand == "Left" && leftHandSwingSound != null)
        {
            audioSource.pitch = 1.0f; // เสียงปกติสำหรับแขนซ้าย
            audioSource.PlayOneShot(leftHandSwingSound);
        }
        else if (hand == "Right" && rightHandSwingSound != null)
        {
            // ปรับให้เสียงทุ้มลงเล็กน้อย (0.8 - 0.9) เพื่อให้รู้สึกถึงแรงทุบที่หนักหน่วง
            audioSource.pitch = 0.85f;
            audioSource.PlayOneShot(rightHandSwingSound);
            Debug.Log("Right Hand Smash!");
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