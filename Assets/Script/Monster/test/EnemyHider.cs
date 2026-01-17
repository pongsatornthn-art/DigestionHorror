using UnityEngine;

public class EnemyHider : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;           // ลากตัว Player มาใส่ช่องนี้
    public LayerMask obstacleMask;     // เลือก Layer เดียวกับที่เป็นกำแพง (Wall)

    [Header("Settings (ต้องตรงกับ VisionFOV)")]
    public float viewDistance = 10f;   // ระยะมองเห็น (ตั้งให้เท่ากับตัวผู้เล่น)
    public float fovAngle = 140f;      // มุมมอง (ตั้งให้เท่ากับตัวผู้เล่น)

    private SpriteRenderer spr;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        // 1. หาความห่าง และ ทิศทางจากผู้เล่นมาหาศัตรู
        Vector3 dirFromPlayer = transform.position - player.position;
        float distance = dirFromPlayer.magnitude;

        // --- เงื่อนไขการซ่อนตัว ---

        // A. ถ้าอยู่ไกลเกินระยะสายตา -> ซ่อน
        if (distance > viewDistance)
        {
            Hide();
            return;
        }

        // B. ถ้าไม่อยู่ในมุมมองด้านหน้า (อยู่นอกกรอบสามเหลี่ยม) -> ซ่อน
        // (ใช้ Vector3.Angle เช็คว่าทำมุมกับด้านหน้าผู้เล่นกี่องศา)
        float angle = Vector3.Angle(player.up, dirFromPlayer);
        if (angle > fovAngle / 2)
        {
            Hide();
            return;
        }

        // C. ถ้าระยะถึง มุมได้ แต่มี "กำแพงบัง" -> ซ่อน
        // ยิง Raycast จากตัวผู้เล่น มาหาตัวศัตรู
        RaycastHit2D hit = Physics2D.Raycast(player.position, dirFromPlayer.normalized, distance, obstacleMask);
        if (hit.collider != null)
        {
            // ถ้าชนกำแพง (Obstacle) ก่อนถึงตัวศัตรู แปลว่าโดนบัง
            Hide();
        }
        else
        {
            // ถ้าผ่านทุกข้อแสดงว่า "มองเห็น"
            Show();
        }
    }

    void Hide()
    {
        spr.enabled = false; // ปิดรูป
    }

    void Show()
    {
        spr.enabled = true;  // เปิดรูป
    }
}