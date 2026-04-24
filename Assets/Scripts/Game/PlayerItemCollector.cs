using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null)
            {
                // Add item to inventory and destroy the item game object
                bool itemAdded = inventoryController.AddItem(collision.gameObject);
                
                if (itemAdded)
                {
                    Sprite icon = collision.GetComponent<UnityEngine.UI.Image>()?.sprite;
                    ItemPickupUIController.Instance?.ShowItemPickup(item.Name, icon);
                    item.Pickup();
                    Destroy(collision.gameObject);
                }
            }
        }
    }

}
