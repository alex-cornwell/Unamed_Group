using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10;

    private ItemDictionary itemDictionary;

    private Key[] hotbarKeys;

    private void Awake()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for ( int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? Key.Digit1 + i : Key.Digit0; // Assign keys 1-9 and 0 for the hotbar slots
        }
    }

    // Update is called once per frame
    void Update()
    {
        for ( int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                // Use Item
                UseItemSlot(i);
            }
        }
    }

    void UseItemSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                hotbarData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
        }

        return hotbarData;
    }

    public void SetHotbaritems(List<InventorySaveData> inventorySaveData)
    {
        // Clear existing items from slots
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        // Populate slots with saved items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Center the item in the slot
                    slot.currentItem = item; // Assign the item to the slot's currentItem variable
                }
            }
        }
    }
}
