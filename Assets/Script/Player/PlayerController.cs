using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    private Rigidbody2D rb;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float currentSpeed;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public bool isRunning;

    [Header("Combat")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 20;

    private Vector2 movement;
    private Vector2 mousePos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = walkSpeed;
        currentStamina = maxStamina;

        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // 1. รับค่าการเดิน
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. รับค่าตำแหน่งเมาส์
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // 3. ระบบวิ่ง
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && movement.magnitude > 0)
        {
            currentSpeed = runSpeed;
            isRunning = true;
            currentStamina -= 10f * Time.deltaTime;
        }
        else
        {
            currentSpeed = walkSpeed;
            isRunning = false;
            if (currentStamina < maxStamina) currentStamina += 5f * Time.deltaTime;
        }

        // 4. ระบบโจมตี
        if (Input.GetButtonDown("Fire1"))
        {
            Attack(); // เรียกใช้ฟังก์ชัน Attack (ที่อยู่ข้างล่าง)
        }
    } // <--- ปิดปีกกา Update ตรงนี้ก่อน!

    void FixedUpdate()
    {
        // --- ส่วนการเคลื่อนที่ ---
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);

        // --- ส่วนการหมุนตัว ---
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    // --- ฟังก์ชัน Attack (เวอร์ชันใหม่) ต้องอยู่นอก Update ---
    void Attack()
    {
        // สร้างวงกลมโจมตี
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // 1. หา Script "EnemyStats" จากตัวที่โดนตี
            EnemyStats enemyStats = enemyCollider.GetComponent<EnemyStats>();

            // 2. ถ้ามี Script นี้จริง ให้ลดเลือด
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(attackDamage);
            }

            // (ถ้าจะทำลายของที่ไม่มีเลือด เช่น ไห/กล่อง ให้ใส่ else ตรงนี้ได้)
        }
    }

    public bool IsMoving()
    {
        return movement.magnitude > 0;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}