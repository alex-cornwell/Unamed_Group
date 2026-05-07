using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance { get; private set; }

    private Dictionary<int, int> itemsCountCache = new Dictionary<int, int>();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
    }

    void Start()
    {
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + 1;
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public bool AddItem(GameObject itemPrefab)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                newItem.GetComponent<RectTransform>().localScale = Vector3.one;
                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }
        Debug.Log("Inventory is full!");
        return false;
    }

    public bool AddItemByID(int itemID)
    {
        if (itemDictionary == null)
            itemDictionary = FindAnyObjectByType<ItemDictionary>();

        GameObject itemPrefab = itemDictionary.GetItemPrefab(itemID);
        if (itemPrefab == null) return false;

        if (itemPrefab.GetComponent<RectTransform>() == null)
        {
            Debug.LogWarning($"ItemDictionary ID {itemID} points to a world prefab, not a UI prefab");
            return false;
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                newItem.GetComponent<RectTransform>().localScale = Vector3.one;
                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }
        Debug.Log("Inventory is full!");
        return false;
    }

    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0) break;
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == itemID)
                {
                    SafeDestroy(slot.currentItem);
                    slot.currentItem = null;
                    amountToRemove--;
                }
            }
        }
        RebuildItemCounts();
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                    invData.Add(new InventorySaveData
                    {
                        itemID    = item.ID,
                        slotIndex = slotTransform.GetSiblingIndex()
                    });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        if (inventorySaveData == null) inventorySaveData = new List<InventorySaveData>();

        if (itemDictionary == null)
            itemDictionary = FindAnyObjectByType<ItemDictionary>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null) slot.currentItem = null;

            for (int i = slotTransform.childCount - 1; i >= 0; i--)
                SafeDestroy(slotTransform.GetChild(i).gameObject);
        }

        int currentSlotCount = inventoryPanel.transform.childCount;
        if (currentSlotCount < slotCount)
        {
            for (int i = currentSlotCount; i < slotCount; i++)
                Instantiate(slotPrefab, inventoryPanel.transform);
        }
        else if (currentSlotCount > slotCount)
        {
            for (int i = currentSlotCount - 1; i >= slotCount; i--)
                SafeDestroy(inventoryPanel.transform.GetChild(i).gameObject);
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex >= inventoryPanel.transform.childCount) continue;
            if (itemDictionary == null) return;

            Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
            if (slot == null) continue;

            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab != null)
            {
                GameObject item = Instantiate(itemPrefab, slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                item.GetComponent<RectTransform>().localScale = Vector3.one;
                slot.currentItem = item;
            }
        }

        RebuildItemCounts();
        Canvas.ForceUpdateCanvases();
    }

    private void SafeDestroy(GameObject obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        DestroyImmediate(obj);
#else
        Destroy(obj);
#endif
    }
}
