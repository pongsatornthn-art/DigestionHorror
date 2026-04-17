using UnityEngine;

public class EnemyKid : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    private bool isPlayerDetected = false;

    [Header("Target")]
    public Transform player;

    [Header("Roam Settings (ระบบเดินสุ่ม)")]
    private Vector2 roamDir;
    private float roamTimer;

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
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        PickNewRoamDirection(); // สุ่มทิศทางเดินตั้งแต่เริ่ม
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        FieldOfViewCheck();

        if (isPlayerDetected)
        {
            if (distanceToPlayer > detectionRange * 1.5f || !CanSeePlayerNow())
            {
                isPlayerDetected = false;
                return;
            }

            HandleMovement(distanceToPlayer);
            HandleCombat();
        }
        else
        {
            HandlePatrol(); // เข้าสู่โหมดเดินสุ่ม
        }
    }

    // --- ระบบเดินสุ่ม (Random Roam) ---
    void PickNewRoamDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        roamDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
        roamTimer = Random.Range(2f, 4f); // สุ่มเวลาเดิน 2-4 วินาที
    }

    void HandlePatrol()
    {
        roamTimer -= Time.deltaTime;

        // เช็คว่ามีกำแพงขวางหน้าในระยะ 1.5 หน่วยไหม
        RaycastHit2D hit = Physics2D.Raycast(transform.position, roamDir, 1.5f, obstructionMask);

        // ถ้าชนกำแพง หรือหมดเวลาเดิน ให้สุ่มทิศใหม่ทันที
        if (hit.collider != null || roamTimer <= 0f)
        {
            PickNewRoamDirection();
        }

        rb.linearVelocity = roamDir * (moveSpeed * 0.7f);
        anim.SetBool("isWalking", true);

        // หันหน้าตามทิศที่เดิน
        float angle = Mathf.Atan2(roamDir.y, roamDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 5f);
    }

    bool CanSeePlayerNow()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return !Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask);
    }

    void FieldOfViewCheck()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            if (Vector2.Angle(transform.up, directionToPlayer) < viewAngle / 2)
            {
                if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    isPlayerDetected = true;
                }
            }
        }
    }

    void HandleMovement(float distanceToPlayer)
    {
        Vector2 moveDir = Vector2.zero;

        if (distanceToPlayer < retreatDistance)
        {
            moveDir = (transform.position - player.position).normalized;
            Vector2 offset = transform.position - player.position;
            currentAngle = Mathf.Atan2(offset.y, offset.x);
        }
        else
        {
            currentAngle += orbitSpeed * Time.deltaTime;
            float targetX = player.position.x + Mathf.Cos(currentAngle) * orbitDistance;
            float targetY = player.position.y + Mathf.Sin(currentAngle) * orbitDistance;
            Vector2 targetPosition = new Vector2(targetX, targetY);
            moveDir = (targetPosition - (Vector2)transform.position).normalized;
        }

        rb.linearVelocity = moveDir * moveSpeed;

        Vector2 lookDir = (Vector2)player.position - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void HandleCombat()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);

        if (Time.time > nextThrowTime)
        {
            anim.Play("Kid_attack");
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
            if (rockRb != null) rockRb.AddForce(dirToPlayer * throwForce, ForceMode2D.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // วาดเส้น Raycast เช็คกำแพงให้เห็นตอนเล่น
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, roamDir * 1.5f);
    }
}