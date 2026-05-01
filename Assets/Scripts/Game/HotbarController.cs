using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 5;

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;

    private void Awake()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
            hotbarKeys[i] = i < 4 ? Key.Digit1 + i : Key.Digit0;
    }

    void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
                UseItemSlot(i);
        }
    }

    void UseItemSlot(int index)
    {
        if (index >= hotbarPanel.transform.childCount) return;
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot?.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            // item.UseItem();
        }
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                    hotbarData.Add(new InventorySaveData
                    {
                        itemID    = item.ID,
                        slotIndex = slotTransform.GetSiblingIndex()
                    });
            }
        }
        return hotbarData;
    }

    public void SetHotbarItems(List<InventorySaveData> hotbarSaveData)
    {
        if (hotbarSaveData == null) hotbarSaveData = new List<InventorySaveData>();

        if (itemDictionary == null)
            itemDictionary = FindAnyObjectByType<ItemDictionary>();

        // Clear only items from slots — slots are pre-built and never touched
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                SafeDestroy(slot.currentItem);
                slot.currentItem = null;
            }
        }

        // Populate saved items into pre-built slots
        foreach (InventorySaveData data in hotbarSaveData)
        {
            if (data.slotIndex >= hotbarPanel.transform.childCount) continue;
            if (itemDictionary == null) return;

            Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
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
