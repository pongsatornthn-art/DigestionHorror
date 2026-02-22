using UnityEngine;

public class EnemyChaser : MonoBehaviour
{


    [Header("FOV Settings")]
    public float viewAngle = 90f;          // องศาความกว้างของสายตา
    public LayerMask obstacleMask;         // Layer ของกำแพง/สิ่งกีดขวาง
    private bool canSeePlayer;             // สถานะว่าเห็นตัวผู้เล่นจริงๆ หรือไม่

    [Header("การตั้งค่า")]
    public Transform player;
    public float moveSpeed = 3f; // ความเร็ว
    public float detectionRadius = 5f; // ⭐ ระยะมองเห็น (ต้องเข้าใกล้กว่านี้ถึงจะไล่)
    public float damage = 10f;

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Combat Settings")]
    public float knockbackForce = 10f; // แรงกระเด็น
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // หาตัว Player อัตโนมัติ
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
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            movement = direction;
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    private void FieldOfViewCheck()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            // คำนวณหาทิศทางที่ศัตรูกำลังหันหน้าไป (สมมติว่าหันตามการเดิน หรือใช้ทิศทางจาก Sprite)
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // เช็คว่า Player อยู่ในองศาการมองเห็นหรือไม่
            // ใช้ Vector2.up หรือทิศหน้าของศัตรู (เช่น transform.up)
            if (Vector2.Angle(transform.up, directionToPlayer) < viewAngle / 2)
            {
                // เช็คว่ามีกำแพงกั้นกลางระหว่าง ศัตรู กับ Player หรือไม่
                if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
        }
        canSeePlayer = false;
    }

    void FixedUpdate()
    {
        // ต้องเรียกใช้ moveCharacter เพื่อให้ Rigidbody ทำงาน
        if (movement != Vector2.zero)
        {
            moveCharacter(movement);
        }
    }

    void moveCharacter(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position + (direction * moveSpeed * Time.fixedDeltaTime));

        // ถ้ามีการเคลื่อนที่ ให้หันหน้าไปทางนั้น
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // 1. เพิ่มค่าการย่อยสลาย (HP ระบบเดิม)
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(damage);
            }

            // 2. ทำให้ Player รับความเสียหายและกระเด็น
            PlayerController playerCtrl = other.gameObject.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                // ลดเลือด HP (แปลง damage เป็น int)
                playerCtrl.PlayerTakeDamage((int)damage);

                // คำนวณทิศทาง: จากตัวศัตรู -> ไปหาผู้เล่น
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                playerCtrl.ApplyKnockback(knockbackDir * knockbackForce);

                Debug.Log("โดนกัด! ผู้เล่นกระเด็นถอยหลัง");
            }
        }
    }

    // ⭐ ฟังก์ชันวาดเส้นวงกลมให้เห็นในหน้า Scene (ช่วยให้ปรับระยะง่ายขึ้น)
    void OnDrawGizmos()
    {
        // วาดรัศมีการมองเห็นเป็นสีเหลือง
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // วาดเส้นกรวยสายตา
        Vector3 forward = transform.up;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRadius);
    }

    private Vector3 DirectionFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal) angleInDegrees += transform.eulerAngles.z;
        return new Vector3(Mathf.Cos((angleInDegrees + 90) * Mathf.Deg2Rad), Mathf.Sin((angleInDegrees + 90) * Mathf.Deg2Rad), 0);
    }
}