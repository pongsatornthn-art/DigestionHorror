using UnityEngine;

public partial class EnemySpirit : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float moveSpeed = 2f;
    public float fleeSpeed = 6f;
    public float damage = 15f;

    [Header("Vision Settings")]
    public float detectionRadius = 8f; // ระยะวงกลมรอบตัว
    [Range(0, 360)]
    public float viewAngle = 90f;      // องศาการมองเห็น (กรวยสายตา)
    public LayerMask obstacleMask;    // เลเยอร์ของกำแพง (ถ้ามี เพื่อไม่ให้มองทะลุ)

    private bool isScaredOfLight = false;
    private float scaredTimer = 0f;
    private bool canSeePlayer = false;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // เช็คการมองเห็น
        canSeePlayer = CheckPlayerInFOV();

        // การตัดสินใจของ AI
        if (isScaredOfLight || scaredTimer > 0)
        {
            // วิ่งหนีออกจาก Player
            Vector2 fleeDir = (Vector2)transform.position - (Vector2)player.position;
            transform.Translate(fleeDir.normalized * fleeSpeed * Time.deltaTime, Space.World);
            scaredTimer -= Time.deltaTime;
        }
        else if (canSeePlayer)
        {
            // ถ้าเห็น Player ให้ไล่ล่า
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // หมุนหน้าไปทางที่เดิน (เพื่อให้กรวยสายตาหมุนตาม)
            Vector2 moveDir = (Vector2)player.position - (Vector2)transform.position;
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        isScaredOfLight = false;
    }

    // ฟังก์ชันเช็คว่าผู้เล่นอยู่ในกรวยสายตาหรือไม่
    bool CheckPlayerInFOV()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            // เช็คว่ามุมระหว่าง "หน้าของศัตรู" กับ "ตัวผู้เล่น" อยู่ในองศาที่กำหนดไหม
            if (Vector2.Angle(transform.up, dirToPlayer) < viewAngle / 2f)
            {
                // (Optional) เช็ค Raycast เพื่อไม่ให้มองทะลุกำแพง
                if (!Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // วาด Gizmos เพื่อดูระยะในหน้า Scene
    void OnDrawGizmosSelected()
    {
        // วาดวงกลมระยะตรวจจับ
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // วาดเส้นกรวยสายตา
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    // --- ส่วนของ Flashlight (คงเดิมตามที่คุณเขียนมา) ---
    public void Scare() { isScaredOfLight = true; scaredTimer = 0.5f; }
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("FlashlightLight") || other.CompareTag("Flashlight"))
        {
            isScaredOfLight = true;
            scaredTimer = 0.2f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. เช็คว่าชนกับ Player หรือไม่
        if (other.CompareTag("Player"))
        {
            // ทำความเสียหายผ่านระบบ Digestion (ระบบเดิมของคุณ)
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(damage);
            }

            // 2. ทำความเสียหายต่อ HP ของ Player โดยตรง
            PlayerController playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                // กำหนดค่าความเสียหายที่ต้องการ (ตัวอย่างเช่น 10)
                int hpDamage = 10;
                playerScript.PlayerTakeDamage(hpDamage);

                Debug.Log("Enemy Spirit โจมตีผู้เล่นและหายไป!");
            }

            // 3. ทำลาย Enemy Spirit ทันทีเมื่อโจมตีสำเร็จ
            Destroy(gameObject);
        }
    }
}