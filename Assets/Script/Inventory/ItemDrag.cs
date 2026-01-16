using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    public InventorySlot mySlot;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mySlot = GetComponentInParent<InventorySlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GetComponent<Image>().color.a == 0) return;

        Debug.Log("1. เริ่มลากแล้ว! (OnBeginDrag)"); // ⭐ เช็คจุดที่ 1
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false; // ปิดการบัง
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (transform.parent == transform.root)
            rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("3. จบการลาก (OnEndDrag)"); // ⭐ เช็คจุดที่ 3
        transform.SetParent(originalParent);
        rectTransform.localPosition = Vector3.zero;
        canvasGroup.blocksRaycasts = true; // เปิดการบังคืน
        canvasGroup.alpha = 1f;

        Inventory.instance.onItemChangedCallback.Invoke();
    }

    // ⭐⭐ ฟังก์ชันนี้สำคัญที่สุดสำหรับการวาง ⭐⭐
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("2. วางลงแล้วจ้า! (OnDrop)"); // ⭐ เช็คจุดที่ 2

        ItemDrag otherItem = eventData.pointerDrag.GetComponent<ItemDrag>();
        if (otherItem != null)
        {
            InventorySlot sourceSlot = otherItem.mySlot;
            InventorySlot targetSlot = this.mySlot;

            Debug.Log($"พยายามสลับจากช่อง {sourceSlot.slotIndex} ไปช่อง {targetSlot.slotIndex}");

            Inventory.instance.SwapItems(sourceSlot.slotIndex, targetSlot.slotIndex);
        }
    }
}