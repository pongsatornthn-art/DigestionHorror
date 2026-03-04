using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ยิ่งเลขเยอะ ผู้เล่นยิ่งลากช้าลง (เช่น 2 คือความเร็วหายไปครึ่งนึง)")]
    public float weight = 2f;

    [Header("Effects")]
    public AudioSource audioSource;
    public AudioClip scrapingSound;
    public ParticleSystem dustParticles;

    private Rigidbody2D rb;
    private bool isBeingDragged = false;
    private bool hasBeenMoved = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ทำให้ของมันฝืดๆ จะได้ไม่ลื่นปรื๊ดเวลาโดนชนปกติ
        rb.mass = weight * 5f;
        rb.linearDamping = 10f;

        // ล็อกไม่ให้ตู้หมุนเคว้งคว้าง
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void StartDragging()
    {
        isBeingDragged = true;

        // เล่นเอฟเฟกต์ฝุ่นกระจาย (แค่ครั้งแรกครั้งเดียว)
        if (!hasBeenMoved && dustParticles != null)
        {
            dustParticles.Play();
            hasBeenMoved = true;
        }

        // เตรียมเล่นเสียงลาก
        if (audioSource != null && scrapingSound != null)
        {
            audioSource.clip = scrapingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopDragging()
    {
        isBeingDragged = false;

        // หยุดเสียงเมื่อปล่อยมือ
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void Update()
    {
        // เช็คว่าถ้ากำลังจับอยู่ แต่ไม่ได้ขยับตัว ให้หยุดเสียงชั่วคราว
        if (isBeingDragged && audioSource != null)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                if (!audioSource.isPlaying) audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying) audioSource.Pause();
            }
        }
    }
}