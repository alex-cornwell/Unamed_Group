using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    private bool isCollecting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollecting) return; // prevent double trigger
        if (!collision.CompareTag("Item")) return;

        Item item = collision.GetComponent<Item>();
        if (item == null) return;

        BentoItem bento = collision.GetComponent<BentoItem>();
        if (bento != null && (bento.IsBeingEaten || bento.IsPickedUp)) return;

        isCollecting = true;
        bool itemAdded = inventoryController.AddItemByID(item.ID);

        if (itemAdded)
        {
            bento?.MarkAsPickedUp();
            Sprite icon = collision.GetComponent<SpriteRenderer>()?.sprite;
            ItemPickupUIController.Instance?.ShowItemPickup(item.Name, icon);
            Destroy(collision.gameObject);
        }
        
        isCollecting = false;
    }

}
