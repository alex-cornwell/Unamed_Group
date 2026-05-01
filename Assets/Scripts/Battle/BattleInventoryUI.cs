using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform itemSlotParent;
    [SerializeField] private GameObject battleItemSlotPrefab;

    private List<BattleItemEntry> items = new List<BattleItemEntry>();
    private bool itemsConsumed = false;

    [System.Serializable]
    public class BattleItemEntry
    {
        public string itemName;
        public int itemID;
        public int quantity;
        public Sprite icon;
        public int slotIndex;
    }

    public void LoadInventory()
    {
        items.Clear();
        itemsConsumed = false;

        string json = PlayerPrefs.GetString("BattleInventory", "");
        if (string.IsNullOrEmpty(json)) { RefreshUI(); return; }

        BattleInventoryData data = JsonUtility.FromJson<BattleInventoryData>(json);
        if (data == null || data.entries == null) { RefreshUI(); return; }

        items = data.entries;
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in itemSlotParent)
            Destroy(child.gameObject);

        bool hasItems = false;
        foreach (BattleItemEntry entry in items)
        {
            if (entry.quantity <= 0) continue;
            hasItems = true;

            GameObject slot = Instantiate(battleItemSlotPrefab, itemSlotParent);
            TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = entry.quantity > 1
                    ? $"{entry.itemName} x{entry.quantity}"
                    : entry.itemName;

            Button useBtn = slot.GetComponentInChildren<Button>();
            string name = entry.itemName;
            useBtn?.onClick.AddListener(() => BattleManager.Instance.UseItem(name));
        }

        if (!hasItems)
        {
            GameObject empty = Instantiate(battleItemSlotPrefab, itemSlotParent);
            TextMeshProUGUI label = empty.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) { label.text = "-- Nothing --"; label.color = Color.gray; }
            empty.GetComponent<Button>().interactable = false;
        }
    }

    public bool HasItem(string itemName)
    {
        return items.Exists(e => e.itemName == itemName && e.quantity > 0);
    }

    public bool WereItemsConsumed() => itemsConsumed;

    public void ConsumeItem(string itemName)
    {
        BattleItemEntry entry = items.Find(e => e.itemName == itemName && e.quantity > 0);
        if (entry == null) return;

        entry.quantity--;
        itemsConsumed = true;
        SaveInventoryBack();
        RefreshUI();
    }

    private void SaveInventoryBack()
    {
        BattleInventoryData data = new BattleInventoryData { entries = items };
        PlayerPrefs.SetString("BattleInventory", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class BattleInventoryData
{
    public List<BattleInventoryUI.BattleItemEntry> entries;
}
