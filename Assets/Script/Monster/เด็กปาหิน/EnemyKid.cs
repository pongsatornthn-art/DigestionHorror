using UnityEngine;

public class EnemyKid : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float fleeDistance = 6f; // ถ้าระยะใกล้กว่านี้จะหนี
    public float stopDistance = 8f; // ถ้าระยะไกลพอดีๆ จะยืนนิ่งๆ ปาหิน

    [Header("Combat")]
    public GameObject rockPrefab; // ลาก Prefab หินมาใส่
    public float throwForce = 10f; // ความแรงในการปา
    public float throwCooldown = 2f; // ปาทุกๆ กี่วินาที

    private float nextThrowTime;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = Vector2.zero;

        // --- Logic การเดิน ---
        if (distance < fleeDistance)
        {
            // 1. ถ้าใกล้เกินไป -> วิ่งหนี (ทิศตรงข้ามกับ Player)
            direction = (transform.position - player.position).normalized;
        }
        else if (distance > stopDistance)
        {
            // 2. ถ้าไกลเกินไป -> อาจจะเดินเข้าหาหน่อย (Optional) หรือยืนเฉยๆ
            // direction = (player.position - transform.position).normalized;
            direction = Vector2.zero;
        }
        else
        {
            // 3. ระยะกำลังดี -> ยืนนิ่งๆ เตรียมปา
            direction = Vector2.zero;
        }

        movement = direction;

        // --- Logic การปาหิน ---
        // ปาเมื่ออยู่ในระยะ และ คูลดาวน์พร้อม
        if (distance < stopDistance + 2f && Time.time > nextThrowTime)
        {
            ThrowRock();
            nextThrowTime = Time.time + throwCooldown;
        }
    }

    void FixedUpdate()
    {
        if (movement != Vector2.zero)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void ThrowRock()
    {
        if (rockPrefab != null)
        {
            // สร้างหินขึ้นมา
            GameObject rock = Instantiate(rockPrefab, transform.position, Quaternion.identity);

            // หาไทศทางไปหา Player
            Vector2 dirToPlayer = (player.position - transform.position).normalized;

            // สั่งหินพุ่งไป
            Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();
            if (rockRb != null)
            {
                rockRb.AddForce(dirToPlayer * throwForce, ForceMode2D.Impulse);
            }
        }
    }

    // วาดเส้นระยะให้ดูในฉาก
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance); // วงแดง = เขตห้ามเข้า (หนี)

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance); // วงเขียว = ระยะยิง
    }
}