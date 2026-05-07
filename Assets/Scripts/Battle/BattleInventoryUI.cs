using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform  itemSlotParent;
    [SerializeField] private GameObject battleItemSlotPrefab;

    private List<BattleItemEntry> items = new List<BattleItemEntry>();

    [System.Serializable]
    public class BattleItemEntry
    {
        public string itemName;
        public int    itemID;
        public int    quantity;
        public Sprite icon;
        public int    slotIndex;
    }

    // ── Load ─────────────────────────────────────────────────────────────────

    public void LoadInventory()
    {
        items.Clear();

        string json = PlayerPrefs.GetString("BattleInventory", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("BattleInventory is empty");
            RefreshUI();
            return;
        }

        BattleInventoryData data = JsonUtility.FromJson<BattleInventoryData>(json);
        if (data == null || data.entries == null)
        {
            Debug.LogWarning("BattleInventory data null");
            RefreshUI();
            return;
        }

        items = data.entries;
        RefreshUI();
    }

    private void RefreshUI()
    {
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

            Button useBtn = slot.GetComponentInChildren<Button>();
            string name   = entry.itemName;
            useBtn?.onClick.AddListener(() => BattleManager.Instance.UseItem(name));
        }
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public bool HasItem(string itemName) =>
        items.Exists(e => e.itemName == itemName && e.quantity > 0);

    private bool itemsConsumed = false;
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
