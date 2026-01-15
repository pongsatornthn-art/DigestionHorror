using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;           // ลากตัว Player มาใส่
    public LayerMask wallLayer;        // เลือก Layer "Wall" เท่านั้น!

    [Header("Settings")]
    public float viewDistance = 10f;   // ระยะมองเห็น
    public float fovAngle = 140f;      // มุมมอง

    private SpriteRenderer spr;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        // คำนวณทิศทางจาก "ผู้เล่น" ไปหา "ศัตรู"
        Vector3 dirToEnemy = transform.position - player.position;
        float distance = dirToEnemy.magnitude;

        // 1. เช็คระยะทาง (ถ้าไกลไป = ไม่เห็น)
        if (distance > viewDistance)
        {
            Hide();
            return;
        }

        // 2. เช็คตวามกว้างมุมมอง (ถ้าอยู่นอกหางตา = ไม่เห็น)
        float angle = Vector3.Angle(player.up, dirToEnemy);
        if (angle > fovAngle / 2)
        {
            Hide();
            return;
        }

        // ---------------------------------------------------------
        // 3. (จุดสำคัญ) ยิง Raycast เช็คกำแพง + ระบบ Debug
        // ---------------------------------------------------------
        RaycastHit2D hit = Physics2D.Raycast(player.position, dirToEnemy.normalized, distance, wallLayer);

        if (hit.collider != null)
        {
            // --- กรณีชนกำแพง ---
            // พิมพ์บอกใน Console ว่าชนตัวอะไร
            Debug.Log($"<color=red>แสงชน:</color> {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

            // วาดเส้นสีเหลืองในฉากให้เห็นจุดที่ชน
            Debug.DrawLine(player.position, hit.point, Color.yellow);

            Hide(); // สั่งซ่อนตัว
        }
        else
        {
            // --- กรณีไม่ชนอะไรเลย ---
            // พิมพ์บอกว่าทางสะดวก
            Debug.Log("<color=green>แสงไม่ชนกำแพงเลย! (มองเห็นศัตรู)</color>");

            // วาดเส้นสีขาวพุ่งไปหาศัตรู
            Debug.DrawRay(player.position, dirToEnemy.normalized * distance, Color.white);

            Show(); // สั่งปรากฏตัว
        }
    }

    void Hide()
    {
        if (spr.enabled) spr.enabled = false;
    }

    void Show()
    {
        if (!spr.enabled) spr.enabled = true;
    }
}