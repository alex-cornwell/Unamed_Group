using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;

    public virtual void UseItem()
    {
        Debug.Log("Using item: " + Name);
    } 
    
    public virtual void Pickup()
    {
        Sprite ItemIcon = GetComponent<SpriteRenderer>().sprite;
        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, ItemIcon);
        }
    }
}
