using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a panel in BattleScene that shows the player's inventory items
// during battle so they can be used or traded

public class BattleInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform itemSlotParent;  // parent holding item slots
    [SerializeField] private GameObject battleItemSlotPrefab; // prefab for each item slot

    // Runtime inventory mirror — loaded from the scene's InventoryController save
    private List<BattleItemEntry> items = new List<BattleItemEntry>();

    [System.Serializable]
    public class BattleItemEntry
    {
        public string itemName;
        public int itemID;
        public int quantity;
        public Sprite icon;
    }

    // -------------------------------------------------------------------------
    // Load inventory from saved data
    // -------------------------------------------------------------------------

    public void LoadInventory()
    {
        items.Clear();

        // Read from PlayerPrefs battle inventory data saved before scene switch
        string json = PlayerPrefs.GetString("BattleInventory", "");
        if (string.IsNullOrEmpty(json)) return;

        BattleInventoryData data = JsonUtility.FromJson<BattleInventoryData>(json);
        if (data == null || data.entries == null) return;

        items = data.entries;
        RefreshUI();
    }

    private void RefreshUI()
    {
        // Clear existing slots
        foreach (Transform child in itemSlotParent)
            Destroy(child.gameObject);

        foreach (BattleItemEntry entry in items)
        {
            if (entry.quantity <= 0) continue;

            GameObject slot = Instantiate(battleItemSlotPrefab, itemSlotParent);
            TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"{entry.itemName} x{entry.quantity}";

            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && entry.icon != null)
                icon.sprite = entry.icon;

            // Wire use button
            Button useBtn = slot.GetComponentInChildren<Button>();
            string name = entry.itemName;
            useBtn?.onClick.AddListener(() => BattleManager.Instance.UseItem(name));
        }
    }

    // -------------------------------------------------------------------------
    // Item checks
    // -------------------------------------------------------------------------

    public bool HasItem(string itemName)
    {
        return items.Exists(e => e.itemName == itemName && e.quantity > 0);
    }

    public void ConsumeItem(string itemName)
    {
        BattleItemEntry entry = items.Find(e => e.itemName == itemName && e.quantity > 0);
        if (entry == null) return;

        entry.quantity--;

        // Sync back to PlayerPrefs so world inventory reflects the use
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
