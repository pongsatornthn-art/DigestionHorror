using UnityEngine;

public class EnemyKid : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f; // มุมมองเป็นองศา (เช่น 90 คือด้านหน้า)
    public LayerMask targetMask;  // เลือก Layer Player
    public LayerMask obstructionMask; // เลือก Layer Wall/Obstacle
    private bool isPlayerDetected = false;

    [Header("Target")]
    public Transform player;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float orbitDistance = 7f;
    public float retreatDistance = 4f;
    public float orbitSpeed = 1f;

    [Header("Combat")]
    public GameObject rockPrefab;
    public float throwForce = 12f;
    public float throwCooldown = 2f;

    private float nextThrowTime;
    private Rigidbody2D rb;
    private float currentAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // ค้นหา Player อัตโนมัติถ้าไม่ได้ใส่ใน Inspector
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- ระบบการมองเห็น ---
        if (!isPlayerDetected)
        {
            FieldOfViewCheck(); // ฟังก์ชันเช็ค FOV ที่เราเพิ่มเข้าไปใหม่
        }
        else
        {
            // ถ้าเคยเห็นแล้ว แต่ตอนนี้ผู้เล่นหนีไปไกลเกิน (หรือแอบหลังกำแพง) ให้เลิกตามและหยุดโจมตี
            // คุณสามารถเพิ่มการเช็ค Raycast ตรงนี้ได้ถ้าต้องการให้หยุดทันทีที่เข้าที่บัง
            if (distanceToPlayer > detectionRange * 1.5f || !CanSeePlayerNow())
            {
                isPlayerDetected = false;
                rb.linearVelocity = Vector2.zero;
                return; // ออกจากฟังก์ชันทันทีเพื่อไม่ให้ไปถึงส่วนการเคลื่อนที่และโจมตี
            }
        }

        // --- ส่วนนี้จะทำงานเฉพาะตอน isPlayerDetected == true เท่านั้น ---
        if (isPlayerDetected)
        {
            HandleMovement(distanceToPlayer); // จัดการการวิ่งวน
            HandleCombat();   // จัดการการปาหิน
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    bool CanSeePlayerNow()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // เช็คว่ามีกำแพงมาบังระหว่างทางหรือไม่
        if (Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
        {
            return false; // มีกำแพงบัง
        }
        return true; // ยังเห็นอยู่
    }


    void FieldOfViewCheck()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // เช็คมุมมอง (เปรียบเทียบกับทิศทางที่ศัตรูหันหน้าไป ปัจจุบันใช้ Vector2.up เป็นหน้า)
            // หมายเหตุ: หากศัตรูมีการหมุนตัว ให้เปลี่ยน Vector2.up เป็น transform.up
            if (Vector2.Angle(transform.up, directionToPlayer) < viewAngle / 2)
            {
                // เช็คว่ามีกำแพงบังไหม
                if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    isPlayerDetected = true;
                    Debug.Log("Enemy Kid: เจอตัวแล้ว!");
                }
            }
        }
    }

    void HandleMovement(float distanceToPlayer)
    {
        Vector2 moveDir = Vector2.zero;

        if (distanceToPlayer < retreatDistance)
        {
            // ถอยหลังหนี
            moveDir = (transform.position - player.position).normalized;
            Vector2 offset = transform.position - player.position;
            currentAngle = Mathf.Atan2(offset.y, offset.x);
        }
        else
        {
            // วิ่งวนรอบผู้เล่น (Orbit)
            currentAngle += orbitSpeed * Time.deltaTime;
            float targetX = player.position.x + Mathf.Cos(currentAngle) * orbitDistance;
            float targetY = player.position.y + Mathf.Sin(currentAngle) * orbitDistance;
            Vector2 targetPosition = new Vector2(targetX, targetY);
            moveDir = (targetPosition - (Vector2)transform.position).normalized;
        }

        rb.linearVelocity = moveDir * moveSpeed;

        // หันหน้าไปหา Player ตลอดเวลาเมื่อเห็นแล้ว
        Vector2 lookDir = (Vector2)player.position - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void HandleCombat()
    {
        // โจมตีเฉพาะเมื่ออยู่ในสถานะ Detected และถึงเวลา Cooldown
        if (isPlayerDetected && Time.time > nextThrowTime)
        {
            ThrowRock();
            nextThrowTime = Time.time + throwCooldown;
        }
    }

    void ThrowRock()
    {
        if (rockPrefab != null)
        {
            GameObject rock = Instantiate(rockPrefab, transform.position, Quaternion.identity);
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();
            if (rockRb != null)
            {
                rockRb.AddForce(dirToPlayer * throwForce, ForceMode2D.Impulse);
            }
        }
    }

    // วาด Gizmos เพื่อให้เห็นระยะในหน้า Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // วาดเส้นขอบเขตการมองเห็น
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRange);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }
}