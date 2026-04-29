using UnityEngine;

// Attach this to a GameObject in SampleScene (e.g. GameController)
// It checks on scene load if a drive belt should be dropped

public class WorldBattleReturn : MonoBehaviour
{
    [SerializeField] private GameObject driveBeltWorldPrefab;   // DriveBeltWorld prefab
    [SerializeField] private Transform dropLocation;            // where to drop it (optional)

    private void Start()
    {
        CheckBattleReturn();
    }

    private void CheckBattleReturn()
    {
        // Check if drive belt should be dropped
        int dropBelt = PlayerPrefs.GetInt("DropDriveBelt", 0);
        if (dropBelt == 1)
        {
            PlayerPrefs.SetInt("DropDriveBelt", 0);
            PlayerPrefs.Save();
            DropDriveBelt();
        }

        // Sync inventory changes from battle back to world inventory
        SyncInventoryFromBattle();
    }

    private void DropDriveBelt()
    {
        if (driveBeltWorldPrefab == null) return;

        // Find player position for drop location
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 pos = dropLocation != null
            ? dropLocation.position
            : (player != null ? player.transform.position + Vector3.right * 2f : Vector3.zero);

        GameObject dropped = Instantiate(driveBeltWorldPrefab, pos, Quaternion.identity);
        dropped.GetComponent<BounceEffect>()?.StartBounce();
    }

    private void SyncInventoryFromBattle()
    {
        // Read updated inventory from PlayerPrefs and apply to world inventory
        string json = PlayerPrefs.GetString("BattleInventory", "");
        if (string.IsNullOrEmpty(json)) return;

        BattleInventoryData data = JsonUtility.FromJson<BattleInventoryData>(json);
        if (data == null || data.entries == null) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        ItemDictionary dict = FindFirstObjectByType<ItemDictionary>();
        if (inventory == null || dict == null) return;

        // Build new inventory list from battle data
        List<InventorySaveData> newInvData = new List<InventorySaveData>();
        int slotIndex = 0;

        foreach (var entry in data.entries)
        {
            for (int i = 0; i < entry.quantity; i++)
            {
                newInvData.Add(new InventorySaveData
                {
                    itemID    = entry.itemID,
                    slotIndex = slotIndex++
                });
            }
        }

        inventory.SetInventoryItems(newInvData);

        // Clear battle inventory prefs
        PlayerPrefs.DeleteKey("BattleInventory");
        PlayerPrefs.Save();
    }
}
