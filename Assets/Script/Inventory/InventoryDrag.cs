using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Transform originalParent;
    private Image image;
    private CanvasGroup canvasGroup;

    public static InventoryDrag itemBeingDragged;

    void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ถ้าเป็นสีใส (ช่องว่าง) ไม่ต้องลาก
        if (image.color == Color.clear) return;

        itemBeingDragged = this;
        originalParent = transform.parent;

        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemBeingDragged != null)
            transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }

    // ⭐ แก้ฟังก์ชันนี้: ใช้ slotIndex แทน
    public void OnDrop(PointerEventData eventData)
    {
        InventoryDrag draggedItem = itemBeingDragged;
        if (draggedItem == null) return;

        // หา Slot ต้นทาง และ ปลายทาง
        InventorySlot oldSlot = draggedItem.originalParent.GetComponent<InventorySlot>();
        InventorySlot newSlot = transform.parent.GetComponent<InventorySlot>();

        if (oldSlot != null && newSlot != null)
        {
            // ใช้เลข slotIndex ที่แท้จริงในการสลับ
            Inventory.instance.SwapItems(oldSlot.slotIndex, newSlot.slotIndex);
        }
    }
}