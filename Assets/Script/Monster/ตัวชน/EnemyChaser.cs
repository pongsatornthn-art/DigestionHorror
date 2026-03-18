using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    [Header("ระบบสายตา (FOV Settings)")]
    public float detectionRadius = 5f;         // ระยะมองเห็น (ต้องเข้าใกล้กว่านี้ถึงจะไล่)
    public float viewAngle = 90f;              // องศาความกว้างของสายตา
    public LayerMask obstacleMask;             // Layer ของกำแพง/สิ่งกีดขวาง (เอาไว้บังสายตา)
    private bool canSeePlayer;                 // สถานะว่าเห็นผู้เล่นหรือไม่

    [Header("การเคลื่อนที่ & โจมตี")]
    public Transform player;
    public float moveSpeed = 3f;               // ความเร็วในการวิ่งไล่
    public float damage = 10f;                 // ดาเมจที่ทำได้
    public float knockbackForce = 10f;         // แรงกระเด็นเมื่อชนผู้เล่น

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator anim;                     // ตัวควบคุมอนิเมชั่น

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // ถ้าลืมใส่ตัว Player เข้ามา ให้มันไปหาเองอัตโนมัติจาก Tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        FieldOfViewCheck();

        if (canSeePlayer)
        {
            // ถ้าเห็นผู้เล่น -> คำนวณทิศทางเพื่อวิ่งเข้าหา
            Vector3 direction = player.position - transform.position;
            movement = direction.normalized;

            // ⭐ สับสวิตช์ Animator ให้เล่นท่าเดิน/วิ่ง
            if (anim != null) anim.SetBool("isMoving", true);
        }
        else
        {
            // ถ้าไม่เห็นผู้เล่น -> สั่งหยุดเดิน
            movement = Vector2.zero;

            // ⭐ สับสวิตช์ Animator ให้กลับไปท่ายืนหายใจ (Idle)
            if (anim != null) anim.SetBool("isMoving", false);
        }
    }

    void FixedUpdate()
    {
        // ใช้ FixedUpdate สำหรับจัดการ Rigidbody ฟิสิกส์การเดิน
        if (movement != Vector2.zero)
        {
            moveCharacter(movement);
        }
    }

    void moveCharacter(Vector2 direction)
    {
        // สั่งให้เดินไปยังทิศทางที่กำหนด
        rb.MovePosition((Vector2)transform.position + (direction * moveSpeed * Time.fixedDeltaTime));

        // สั่งให้ตัวศัตรูหันหน้าไปทางที่เดินเสมอ (สำหรับเกม Top-Down 2D)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void FieldOfViewCheck()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // เช็คว่า Player อยู่ในกรวยสายตาหรือไม่ (อ้างอิงจากด้านหน้าของศัตรู transform.up)
            if (Vector2.Angle(transform.up, directionToPlayer) < viewAngle / 2f)
            {
                // ยิงเรดาร์เช็คว่ามีกำแพงบังอยู่ไหม?
                if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
        }
        canSeePlayer = false; // ถ้าไม่อยู่ในระยะ หรือมีกำแพงบัง = มองไม่เห็น
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // เมื่อเดินไปชนผู้เล่น
        if (other.gameObject.CompareTag("Player"))
        {
            // 1. เพิ่มค่าการย่อยสลาย (ถ้ามีระบบ DigestionSystem อยู่)
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(damage);
            }

            // 2. ทำให้ Player เสียเลือดและกระเด็น
            PlayerController playerCtrl = other.gameObject.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.PlayerTakeDamage((int)damage);

                // คำนวณทิศทางให้ผู้เล่นกระเด็นถอยหลังจากตัวผี
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                playerCtrl.ApplyKnockback(knockbackDir * knockbackForce);

                Debug.Log("โดนผีกัด! กระเด็นไปเลย!");
            }
        }
    }

    // ⭐ ฟังก์ชันวาดเส้นกะระยะในหน้าต่าง Scene (มีประโยชน์ตอนนั่งทำด่านมากๆ)
    void OnDrawGizmos()
    {
        // วาดวงกลมรัศมีการมองเห็น (สีเหลือง)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // วาดเส้นกรวยสายตาซ้าย-ขวา (สีแดง)
        Vector3 forward = transform.up;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2f) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRadius);
    }
}