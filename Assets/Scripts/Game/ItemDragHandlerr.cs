using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandlerr : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    Canvas rootCanvas;

    public float minDropDistance = 2f; // Minimum distance from the original slot to allow dropping
    public float maxDropDistance = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; //save  OG parent
        transform.SetParent(rootCanvas.transform); // move to Canvas root, not scene root
        canvasGroup.blocksRaycasts = false; //allow raycast to pass through the dragged item
        canvasGroup.alpha = 0.7f; //make the dragged item semi-transparent
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; //move the item with the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; //restore raycast blocking
        canvasGroup.alpha = 1f; //restore full opacity

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); //check if we are dropping on a slot

        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter; //get the GameObject we are dropping on
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>(); //try to get the slot from the parent of the GameObject
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>(); //get the original slot

        if (dropSlot != null) //if we dropped on a valid slot
        {
            if (dropSlot.currentItem != null) //if the slot already has an item
            {
                //swap items between slots
                dropSlot.currentItem.transform.SetParent(originalParent.transform); //move the existing item back to the original slot
                originalSlot.currentItem = dropSlot.currentItem; //update original slot with the item from the drop slot
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //reset position of the item in the drop slot
            }
            else
            {
                originalSlot.currentItem = null; //clear the original slot
            }

            //move item into drop slot
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject; //update drop slot with the new item
        }
        else
        {
            if(!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                originalSlot.currentItem = null; //clear the original slot if we are dropping back into the inventory
            }
            transform.SetParent(originalParent); //if not dropped on a slot, return to original position
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //reset position to ensure it snaps back to the slot
    }

    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    void DropItem(Slot originalSlot)
    {
        originalSlot.currentItem = null; //clear the original slot
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform; //get player position
        if(playerTransform == null)
        {
            Debug.LogError("Missing 'Player' tag");
            return;
        }
        
        Vector2 dropOffset = Random.insideUnitCircle * Random.Range(minDropDistance, maxDropDistance); //randomize drop position within a circle
        Vector3 dropPosition = (Vector2)playerTransform.position + dropOffset; //calculate drop position

        GameObject dropItem =Instantiate(gameObject, dropPosition, Quaternion.identity); //spawn the item in the world
        dropItem.GetComponent<BounceEffect>().StartBounce();

        Destroy(gameObject); //destroy the dragged item from the inventory
    }
}
