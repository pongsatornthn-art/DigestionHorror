using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : MonoBehaviour
{
    [Header("Settings (ตั้งค่า)")]
    [Tooltip("ยิ่งเลขเยอะ กล่องยิ่งหนักลากยาก")]
    public float weight = 2f;
    [Tooltip("ระยะห่างที่จะให้ผู้เล่นกด E ดึงกล่องได้")]
    public float interactDistance = 1.5f;

    [Header("Effects")]
    public AudioSource audioSource;
    public AudioClip scrapingSound;
    public ParticleSystem dustParticles;

    private Rigidbody2D rb;
    private Transform player;
    private bool isBeingDragged = false;
    private bool hasBeenMoved = false;
    private Vector3 lastPosition;

    private FixedJoint2D joint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = weight * 5f;
        rb.linearDamping = 10f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        lastPosition = transform.position;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }

        // ⭐ เตรียมแผ่นเสียงใส่เครื่องไว้ตั้งแต่เริ่มเกมเลย
        if (audioSource != null && scrapingSound != null)
        {
            audioSource.clip = scrapingSound;
            audioSource.loop = true;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // ==========================================
        // 1. ระบบกดค้างเพื่อลาก (Hold E to Drag)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.E) && distToPlayer <= interactDistance)
        {
            if (!isBeingDragged) StartDragging();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            if (isBeingDragged) StopDragging();
        }

        // ==========================================
        // ⭐ 2. ระบบจัดการเสียง (อัปเกรดใหม่)
        // ==========================================
        if (audioSource != null)
        {
            // วัดระยะทางว่ากล่องขยับจากเฟรมที่แล้วไหม
            float moveDistance = Vector3.Distance(transform.position, lastPosition);

            // 🎯 ถ้ากล่องมีการขยับ (ไม่ว่าจะ ดึง หรือ ดัน หรือกระเด็น!)
            if (moveDistance > 0.005f)
            {
                if (!audioSource.isPlaying) audioSource.Play();

                // ปล่อยฝุ่นฟุ้งตอนขยับครั้งแรกด้วย
                if (!hasBeenMoved && dustParticles != null)
                {
                    dustParticles.Play();
                    hasBeenMoved = true;
                }
            }
            else // ถ้ากล่องหยุดนิ่งสนิท
            {
                if (audioSource.isPlaying) audioSource.Pause();
            }

            // จดจำตำแหน่งปัจจุบันไว้เทียบในเฟรมหน้า
            lastPosition = transform.position;
        }
    }

    public void StartDragging()
    {
        isBeingDragged = true;

        // สร้างเชือกผูกติดกับผู้เล่น
        joint = gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = player.GetComponent<Rigidbody2D>();
    }

    public void StopDragging()
    {
        isBeingDragged = false;

        // สับเชือกทิ้ง (ส่วนเสียงไม่ต้องสั่งปิดตรงนี้แล้ว เพราะระบบด้านบนจะเช็คให้เองว่ากล่องหยุดขยับแล้วเสียงจะเงียบไปเอง)
        if (joint != null)
        {
            Destroy(joint);
        }
    }
}