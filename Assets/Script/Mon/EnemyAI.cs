using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 3f;           // ความเร็วการเดิน
    public float stopDistance = 0.5f;  // ระยะหยุด (ไม่ให้เดินทับตัวผู้เล่นเป๊ะๆ)

    private Transform target;          // เป้าหมาย (Player)
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ค้นหาตัวละครที่มี Tag ว่า "Player" โดยอัตโนมัติ
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogError("หา Player ไม่เจอ! อย่าลืมตั้ง Tag 'Player' ให้ตัวละครนะครับ");
        }
    }

    void Update()
    {
        // ถ้าหาผู้เล่นไม่เจอ (หรือผู้เล่นตายไปแล้ว) ให้หยุดทำงาน
        if (target == null) return;

        // คำนวณทิศทาง: (ตำแหน่งผู้เล่น - ตำแหน่งตัวเอง)
        Vector2 direction = target.position - transform.position;

        // แปลงเป็นทิศทางมาตรฐาน (Normalized) เพื่อให้ความเร็วคงที่
        direction.Normalize();
        movement = direction;

        // --- เพิ่มเติม: หันหน้าตามผู้เล่น ---
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // เช็คระยะห่าง ถ้ายังไกลกว่าระยะหยุด ให้เดินเข้าไปหา
        if (Vector2.Distance(transform.position, target.position) > stopDistance)
        {
            MoveCharacter(movement);
        }
    }

    void MoveCharacter(Vector2 direction)
    {
        // ใช้ MovePosition เพื่อให้เดินชนกำแพงแล้วไม่ทะลุ
        rb.MovePosition((Vector2)transform.position + (direction * speed * Time.fixedDeltaTime));
    }
}