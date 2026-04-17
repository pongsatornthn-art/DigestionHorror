using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player == null) return;

        // ให้กล้องวิ่งตามตำแหน่ง Player แต่คงความสูง (Z หรือ Y) เอาไว้
        Vector3 newPosition = player.position;
        newPosition.z = transform.position.z; // ถ้าเป็นเกม 2D ใช้ Z ถ้า 3D ใช้ Y
        transform.position = newPosition;
    }
}