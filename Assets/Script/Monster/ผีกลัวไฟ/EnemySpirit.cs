using UnityEngine;

public partial class EnemySpirit : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Roam Settings (ระบบเดินสุ่ม)")]
    private Vector2 roamDir;
    private float roamTimer;
    public LayerMask obstacleMask; // ⭐ เลเยอร์กำแพง ให้เลือกติ๊ก Wall ใน Inspector ด้วยนะครับ

    [Header("Settings")]
    public float moveSpeed = 2f;      // ความเร็วตอนเดินหาผู้เล่น
    public float rushSpeed = 12f;     // ⭐ ความเร็วตอนเห็นผู้เล่นแล้วพุ่งใส่! (ปรับให้เร็วๆ ได้เลย)
    public float fleeSpeed = 6f;      // ความเร็วตอนโดนไฟส่องแล้ววิ่งหนี
    public float damage = 3f;

    [Header("Detection Settings")]
    public float detectionRadius = 15f;
    public float loseTargetDistance = 20f;
    private bool isChasing = false;

    private bool isScared = false;
    private float scaredTimer = 0f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        PickNewRoamDirection();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // เช็คระยะการมองเห็นผู้เล่น
        if (!isChasing && distanceToPlayer < detectionRadius)
        {
            isChasing = true;
        }
        else if (isChasing && distanceToPlayer > loseTargetDistance)
        {
            isChasing = false;
        }

        // ⭐ ลำดับความสำคัญ: โดนไฟส่อง (กลัว) -> ไล่ล่า (พุ่งใส่) -> ไม่เห็นใคร (เดินสุ่ม)
        if (isScared || scaredTimer > 0)
        {
            FleeFromPlayer();
            SetWalkingAnim(true);
        }
        else if (isChasing)
        {
            ChasePlayer();
            SetWalkingAnim(true);
        }
        else
        {
            Patrol();
        }
    }

    void PickNewRoamDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        roamDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
        roamTimer = Random.Range(2f, 4f);
    }

    void FleeFromPlayer()
    {
        Vector2 fleeDir = (Vector2)transform.position - (Vector2)player.position;
        MoveAndRotate(fleeDir.normalized, fleeSpeed);
        scaredTimer -= Time.deltaTime;
        if (scaredTimer <= 0) isScared = false;
    }

    void ChasePlayer()
    {
        // ⭐ ใช้ rushSpeed เพื่อให้ผีพุ่งใส่ด้วยความเร็วสูง
        Vector2 chaseDir = (Vector2)player.position - (Vector2)transform.position;
        MoveAndRotate(chaseDir.normalized, rushSpeed);
    }

    void Patrol()
    {
        SetWalkingAnim(true); // เดินสุ่มให้แอนิเมชันเดินทำงาน

        roamTimer -= Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, roamDir, 1.5f, obstacleMask);

        if (hit.collider != null || roamTimer <= 0f)
        {
            PickNewRoamDirection();
        }

        MoveAndRotate(roamDir, moveSpeed * 0.5f); // ตอนเดินสุ่มจะเดินช้าๆ
    }

    void MoveAndRotate(Vector2 direction, float speed)
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    // วาดเส้นวงกลมสีเหลืองให้เห็นระยะสายตาตอนคลิกที่ตัวผี
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void SetWalkingAnim(bool isWalking)
    {
        if (anim != null)
        {
            anim.SetBool("isWalking", isWalking);
        }
    }

    public void SetScared()
    {
        // คำสั่งนี้ถูกเรียกจากไฟฉาย (FieldOfViewCheck) ทำให้มันหยุดพุ่ง แล้วเปลี่ยนเป็นวิ่งหนี
        isScared = true;
        scaredTimer = 0.5f;
        isChasing = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.PlayerTakeDamage((int)damage);
                Destroy(gameObject);
            }
        }
    }
}