using UnityEngine;

public class BossRoomAudioZone : MonoBehaviour
{
    [Header("ใส่ Audio Source ของบอสที่นี่")]
    public AudioSource[] bossAudioSources; // ใส่กี่อันก็ได้ ทั้งเสียงทุบ เสียงเพลงบอส

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // เมื่อผู้เล่นเข้าห้อง -> เปิดเสียง
            foreach (AudioSource source in bossAudioSources)
            {
                source.mute = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // เมื่อผู้เล่นออกจากห้อง -> ปิดเสียง (Mute)
            foreach (AudioSource source in bossAudioSources)
            {
                source.mute = true;
            }
        }
    }
}