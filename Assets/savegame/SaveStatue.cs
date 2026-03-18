using UnityEngine;

public class SaveStatue : MonoBehaviour
{
    [Header("การตั้งค่าจุดเซฟ")]
    public float interactDistance = 2.5f;

    // ❌ ไม่ต้องลาก savePanel มาใส่ตรงนี้แล้ว เพราะเราจะดึงจาก GameManager แทน
    private Transform player;
    private bool isNearStatue = false;

    void Start()
    {
        if (PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || GameManager.instance == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= interactDistance)
        {
            isNearStatue = true;

            // ถ้ายืนอยู่ในระยะ แล้วกดปุ่ม E
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleSaveMenu();
            }
        }
        else
        {
            isNearStatue = false;

            // ถ้าเดินห่างออกมาจากรูปปั้น ให้ปิดหน้าต่างอัตโนมัติ
            if (GameManager.instance.savePanel.activeSelf)
            {
                GameManager.instance.ResumeGame();
            }
        }
    }

    void ToggleSaveMenu()
    {
        bool isSaveMenuOpen = GameManager.instance.savePanel.activeSelf;

        if (!isSaveMenuOpen)
        {
            // 1. เคลียร์ UI กระเป๋า/ร้านค้า กันบัง
            if (InventoryUI.instance != null && InventoryUI.instance.inventoryPanel != null)
                InventoryUI.instance.inventoryPanel.SetActive(false);
            if (ShopUI.instance != null && ShopUI.instance.shopPanel != null)
                ShopUI.instance.shopPanel.SetActive(false);

            // 2. สั่ง GameManager ให้หยุดเวลาเกม และเปิดหน้าต่างเซฟ!
            GameManager.instance.PauseGame();
            GameManager.instance.ShowSaveMenu();
        }
        else
        {
            // 3. ปิดหน้าเซฟ และให้เวลาในเกมเดินต่อ
            GameManager.instance.ResumeGame();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}