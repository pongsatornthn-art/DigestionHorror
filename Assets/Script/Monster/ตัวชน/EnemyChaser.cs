using UnityEngine;
using System.Collections;

public class EnemyChaser : MonoBehaviour
{
    [Header("Roam Settings (ระบบเดินสุ่ม)")]
    private Vector2 roamDir;
    private float roamTimer;

    [Header("Stun Settings")]
    public float stunDuration = 2f;
    private bool isStunned = false;

    [Header("FOV Settings")]
    public float viewAngle = 90f;
    public LayerMask obstacleMask; // เอาไว้เช็คกำแพงตอนเดินสุ่มด้วย
    private bool canSeePlayer;

    [Header("การตั้งค่า")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRadius = 5f;
    public float damage = 10f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator anim;

    [Header("Combat Settings")]
    public float knockbackForce = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (isStunned || player == null) return;

        FieldOfViewCheck();

        if (canSeePlayer)
        {
            // โหมดไล่ล่า
            Vector3 direction = player.position - transform.position;
            movement = direction.normalized;

            if (anim != null) anim.SetBool("isMoving", true);
        }
        else
        {
            // โหมดสุ่มเดิน
            Patrol();
        }
    }

    void PickNewRoamDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        roamDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
        roamTimer = Random.Range(2f, 4f);
    }

    private void Patrol()
    {
        roamTimer -= Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, roamDir, 1.5f, obstacleMask);

        if (hit.collider != null || roamTimer <= 0f)
        {
            PickNewRoamDirection();
        }

        movement = roamDir;
        if (anim != null) anim.SetBool("isMoving", true);
    }

    private void FieldOfViewCheck()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            if (Vector2.Angle(transform.up, directionToPlayer) < viewAngle / 2)
            {
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
        if (movement != Vector2.zero && !isStunned)
        {
            moveCharacter(movement);
        }
    }

    void moveCharacter(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position + (direction * (canSeePlayer ? moveSpeed : moveSpeed * 0.7f) * Time.fixedDeltaTime));

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !isStunned)
        {
            if (DigestionSystem.instance != null)
                DigestionSystem.instance.IncreaseDigestion(damage);

            PlayerController playerCtrl = other.gameObject.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.PlayerTakeDamage((int)damage);
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                playerCtrl.ApplyKnockback(knockbackDir * knockbackForce);
            }

            StartCoroutine(StunRoutine());
        }
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        movement = Vector2.zero;
        if (anim != null) anim.SetBool("isMoving", false);
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, roamDir * 1.5f);
    }
}