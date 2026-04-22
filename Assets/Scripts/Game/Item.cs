using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;

    public virtual void Pickup()
    {
        Sprite itemIcon = GetComponent<Image>().sprite; //get the sprite from the item game object
        if(ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon); //show the pickup UI with the item name and icon
        }
    }
}
