using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandlerr : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    Canvas rootCanvas;

    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;
    [SerializeField] private GameObject worldDropPrefab; 

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            rootCanvas = canvas.rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Find canvas at drag time as fallback
        if (rootCanvas == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) rootCanvas = canvas.rootCanvas;
        }

        if (rootCanvas == null)
        {
            Debug.LogError("No Canvas found for item drag");
            return;
        }

        originalParent = transform.parent;
        transform.SetParent(rootCanvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null && eventData.pointerEnter != null)
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();

        Slot originalSlot = originalParent.GetComponent<Slot>();
        if (originalSlot == null)
        {
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        }

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalParent.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            if (!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot, eventData.position);
                return;
            }
            else
            {
                originalSlot.currentItem = null;
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    void DropItem(Slot originalSlot, Vector2 screenPosition)
    {
        originalSlot.currentItem = null;

        // Convert screen position to world position
        Camera cam = Camera.main;
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(
            screenPosition.x, screenPosition.y,
            Mathf.Abs(cam.transform.position.z)));
        worldPos.z = 0f;

        // Use the world prefab if assigned, otherwise skip
        if (worldDropPrefab == null)
        {
            Debug.LogWarning("No world drop prefab assigned on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        GameObject dropped = Instantiate(worldDropPrefab, worldPos, Quaternion.identity);
        dropped.GetComponent<BounceEffect>()?.StartBounce();

        BentoItem bento = dropped.GetComponent<BentoItem>();
        if (bento != null)
            bento.DropOnMap(worldPos);

        Destroy(gameObject);
    }
}