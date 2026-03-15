using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("ลำโพงหลัก 2 ตัว")]
    public AudioSource ambientSource; // สำหรับเสียงสภาพแวดล้อม (ลม, ฝน, แมลง)
    public AudioSource musicSource;   // สำหรับเสียงเพลงเฉพาะสถานที่ (โบสถ์, บ้าน)

    [Header("ตั้งค่า")]
    public float fadeDuration = 1.5f; // เวลาในการค่อยๆ หรี่เสียง (วินาที)

    private Coroutine fadeRoutine;

    void Awake()
    {
        // ทำให้มี AudioManager แค่ตัวเดียวในฉาก
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันสำหรับเปลี่ยนเพลงสถานที่
    public void PlayZoneMusic(AudioClip newMusic)
    {
        // ถ้าเป็นเพลงเดิมอยู่แล้ว ไม่ต้องทำอะไร
        if (musicSource.clip == newMusic) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeMusic(newMusic));
    }

    private IEnumerator FadeMusic(AudioClip newMusic)
    {
        float startVolume = musicSource.volume;

        // 1. ค่อยๆ หรี่เสียงเพลงเก่าลงจนดับ
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }

        // 2. เปลี่ยนไฟล์เพลง และเล่น
        musicSource.clip = newMusic;
        if (newMusic != null) musicSource.Play();
        else musicSource.Stop(); // ถ้าไม่ได้ใส่เพลงมา ก็คือให้เงียบไปเลย

        // 3. ค่อยๆ เร่งเสียงเพลงใหม่ขึ้นมา
        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }
    }
}