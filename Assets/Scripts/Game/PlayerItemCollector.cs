using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    void Start()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Item")) return;

        Item item = collision.GetComponent<Item>();
        if (item == null) return;

        BentoItem bento = collision.GetComponent<BentoItem>();
        if (bento != null && bento.IsBeingEaten) return;
        if (bento != null && !bento.MarkAsPickedUp()) return;

        // Guard for plain Item objects (no BentoItem)
        if (bento == null)
        {
            if (!collision.enabled) return; // already picked up
            collision.enabled = false;
        }

        bool itemAdded = inventoryController.AddItemByID(item.ID);

        if (itemAdded)
        {
            Sprite icon = collision.GetComponent<SpriteRenderer>()?.sprite;
            ItemPickupUIController.Instance?.ShowItemPickup(item.Name, icon);
            Destroy(collision.gameObject);
        }
    }
}