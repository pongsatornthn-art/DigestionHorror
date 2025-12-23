using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // ลากตัว Player มาใส่ช่องนี้
    public float smoothSpeed = 0.125f;  // ความหน่วง (ค่าน้อย = กล้องตามแบบนุ่มๆ)
    public Vector3 offset;         // ระยะห่าง (ปกติ Z ต้องเป็น -10 เพื่อให้เห็นภาพ 2D)

    void LateUpdate()
    {
        if (target == null) return;

        // คำนวณตำแหน่งที่กล้องควรจะไป (ตำแหน่งผู้เล่น + ระยะห่าง)
        // เราล็อคแกน Z ไว้ที่เดิม เพื่อไม่ให้กล้องจมเข้าไปในฉาก
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        // ใช้ Lerp เพื่อให้กล้องค่อยๆ เลื่อนตาม (Smooth) ไม่แข็งทื่อ
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}