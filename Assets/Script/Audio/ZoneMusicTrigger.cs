using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ZoneMusicTrigger : MonoBehaviour
{
    [Header("--- ตอนเดินเข้าโซน ---")]
    [Tooltip("เพลงที่จะเล่นเมื่อผู้เล่นเดินเข้ามาข้างใน")]
    public AudioClip zoneMusic;

    [Header("--- ตอนเดินออกจากโซน ---")]
    public bool changeMusicOnExit = true;

    [Tooltip("เพลงที่จะเล่นเมื่อเดินออกไป (ถ้าปล่อยเป็น None เพลงจะเฟดดับไปเอง เหลือแค่เสียงลม)")]
    public AudioClip musicOnExit;

    // ทำงานเมื่อผู้เล่นเดิน "เข้า" มาในกรอบสีเขียว
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayZoneMusic(zoneMusic);
            }
        }
    }

    // ⭐ [เพิ่มใหม่] ทำงานเมื่อผู้เล่นเดิน "ออก" นอกกรอบสีเขียว
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && changeMusicOnExit)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayZoneMusic(musicOnExit);
            }
        }
    }
}