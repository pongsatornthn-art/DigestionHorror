using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    public InventorySlot mySlot; // ตัวเก็บข้อมูลว่าไอเทมนี้มาจากช่องไหน

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // ✅ หา Slot ตั้งแต่เกิด
        mySlot = GetComponentInParent<InventorySlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ✅ ถ้าหา Slot ไม่เจอ ให้รีบค้นหาใหม่ก่อนเริ่มลาก
        if (mySlot == null) mySlot = GetComponentInParent<InventorySlot>();

        if (mySlot == null || mySlot.item == null) return;

        originalParent = transform.parent;

        // หา Canvas ตัวแม่สุดเพื่อไม่ให้ไอเทมจม
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            transform.SetParent(canvas.rootCanvas.transform, true);
            transform.SetAsLastSibling();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // ปรับให้จางลงตอนลาก จะได้ดูรู้ว่ากำลังย้าย
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ✅ กลับไปที่ Slot เดิมเสมอ (เพราะเราคราฟต์แบบมายคราฟ ของจริงต้องยังอยู่ในกระเป๋า)
        transform.SetParent(originalParent, true);
        rectTransform.localPosition = Vector3.zero;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (Inventory.instance != null && Inventory.instance.onItemChangedCallback != null)
            Inventory.instance.onItemChangedCallback.Invoke();
    }

    // ฟังก์ชันสลับของใน Inventory
    public void OnDrop(PointerEventData eventData)
    {
        ItemDrag otherItem = eventData.pointerDrag.GetComponent<ItemDrag>();
        if (otherItem != null && otherItem != this)
        {
            if (this.mySlot == null || otherItem.mySlot == null) return;
            if (Inventory.instance != null)
            {
                Inventory.instance.SwapItems(otherItem.mySlot.slotIndex, this.mySlot.slotIndex);
            }
        }
    }
}