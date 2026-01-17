using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    [Header("การตั้งค่า")]
    public Transform player;
    public float moveSpeed = 3f; // ความเร็ว
    public float detectionRadius = 5f; // ⭐ ระยะมองเห็น (ต้องเข้าใกล้กว่านี้ถึงจะไล่)
    public float damage = 10f;

    private Rigidbody2D rb;
    private Vector2 movement;

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

        // 1. เช็คระยะห่างระหว่าง ผี กับ คน
        float distance = Vector2.Distance(transform.position, player.position);

        // 2. ถ้าระยะ "น้อยกว่า" ที่กำหนด (แปลว่าอยู่ใกล้) -> ให้เริ่มไล่
        if (distance < detectionRadius)
        {
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            movement = direction;
        }
        else
        {
            // ถ้าอยู่ไกล -> หยุดเดิน
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        // สั่งให้เดิน (ถ้า movement เป็น 0 มันก็จะหยุดเอง)
        moveCharacter(movement);
    }

    void moveCharacter(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position + (direction * moveSpeed * Time.fixedDeltaTime));
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(damage);
                Debug.Log("โดนกัด!");
            }
        }
    }

    // ⭐ ฟังก์ชันวาดเส้นวงกลมให้เห็นในหน้า Scene (ช่วยให้ปรับระยะง่ายขึ้น)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}