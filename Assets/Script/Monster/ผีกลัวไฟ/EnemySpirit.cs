using UnityEngine;

public class EnemySpirit : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float moveSpeed = 2f;    // ความเร็วปกติ
    public float fleeSpeed = 6f;    // ความเร็วตอนหนี
    public float damage = 15f;      // ดาเมจ Digestion

    [Header("ระยะมองเห็น")]
    public float detectionRadius = 8f; // ⭐ ต้องเข้ามาใกล้กว่านี้ ผีถึงจะเริ่มไล่

    private bool isScaredOfLight = false;
    private float scaredTimer = 0f; // ตัวช่วยนับเวลาหนี (กันกระตุก)

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

        // คำนวณระยะห่าง
        float distance = Vector2.Distance(transform.position, player.position);

        // กรณี 1: โดนไฟฉาย (สำคัญสุด ต้องหนีก่อน)
        if (isScaredOfLight || scaredTimer > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, -fleeSpeed * Time.deltaTime);
            scaredTimer -= Time.deltaTime; // ลดเวลาหนี
        }
        // กรณี 2: ไม่โดนไฟ แต่ผู้เล่นอยู่ในระยะ (ไล่ล่า)
        else if (distance < detectionRadius)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        // กรณี 3: อยู่ไกลเกินไป (ยืนนิ่งๆ)
        else
        {
            // ไม่ทำอะไร หรือจะใส่ให้เดินเล่นไปมาก็ได้
        }

        isScaredOfLight = false; // รีเซ็ตค่าทุกเฟรม รอรับ trigger ใหม่
    }

    // ฟังก์ชันวาดวงกลมใน Scene ให้เห็นระยะ (ตอนแก้จะได้กะถูก)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // ---------------------------------------------------------
    // ส่วนของการชน (Collision / Trigger)
    // ---------------------------------------------------------

    public void Scare()
    {
        isScaredOfLight = true;
        scaredTimer = 0.5f; // ให้หนีต่ออีก 0.5 วิ
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ชน Player -> เพิ่มค่า Digestion
        if (other.CompareTag("Player"))
        {
            if (DigestionSystem.instance != null)
            {
                DigestionSystem.instance.IncreaseDigestion(damage);
            }
        }
    }

    // ใช้ OnTriggerStay เพื่อเช็คตลอดเวลาที่แสงแช่อยู่
    void OnTriggerStay2D(Collider2D other)
    {
        // ชนแสงไฟ (ต้องตั้ง Tag ของแสงไฟว่า FlashlightLight หรือ Flashlight ให้ตรงกันนะ)
        if (other.CompareTag("FlashlightLight") || other.CompareTag("Flashlight"))
        {
            isScaredOfLight = true;
            scaredTimer = 0.2f; // รีเซ็ตเวลาหนีเรื่อยๆ ตราบใดที่ยังโดนแสง
        }
    }
}