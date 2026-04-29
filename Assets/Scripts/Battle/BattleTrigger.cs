using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string enemyDataName = "MenehuneData";
    [SerializeField] private bool isMenehuneLeader = false;

    private bool battleStarted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (battleStarted) return;
        if (!collision.CompareTag("Player")) return;
        StartBattle();
    }

    private void StartBattle()
    {
        battleStarted = true;

        // Save which enemy and return scene
        PlayerPrefs.SetString("CurrentEnemy", enemyDataName);
        PlayerPrefs.SetString("ReturnScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsMenehuneLeader", isMenehuneLeader ? 1 : 0);

        // Save inventory to PlayerPrefs so BattleScene can read it
        SaveInventoryToPrefs();

        PlayerPrefs.Save();
        SceneManager.LoadScene(battleSceneName);
    }

    private void SaveInventoryToPrefs()
    {
        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null) return;

        // Build a simple item count list
        BattleInventoryData data = new BattleInventoryData();
        data.entries = new List<BattleInventoryUI.BattleItemEntry>();

        Dictionary<string, BattleInventoryUI.BattleItemEntry> itemCounts = 
            new Dictionary<string, BattleInventoryUI.BattleItemEntry>();

        List<InventorySaveData> invItems = inventory.GetInventoryItems();
        foreach (InventorySaveData invItem in invItems)
        {
            // Get item name from ItemDictionary
            ItemDictionary dict = FindFirstObjectByType<ItemDictionary>();
            if (dict == null) continue;

            GameObject prefab = dict.GetItemPrefab(invItem.itemID);
            if (prefab == null) continue;

            Item item = prefab.GetComponent<Item>();
            if (item == null) continue;

            if (itemCounts.ContainsKey(item.Name))
            {
                itemCounts[item.Name].quantity++;
            }
            else
            {
                itemCounts[item.Name] = new BattleInventoryUI.BattleItemEntry
                {
                    itemName = item.Name,
                    itemID   = item.ID,
                    quantity = 1
                };
            }
        }

        data.entries.AddRange(itemCounts.Values);
        PlayerPrefs.SetString("BattleInventory", JsonUtility.ToJson(data));
    }
}
