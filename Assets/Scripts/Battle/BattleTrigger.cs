using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string enemyDataName   = "MenehuneData";
    [SerializeField] private bool isMenehuneLeader  = false;

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

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerReturnX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerReturnY", player.transform.position.y);
            PlayerPrefs.SetInt("HasReturnPosition", 1);
        }

        PlayerPrefs.SetFloat("MenehuneX", transform.position.x);
        PlayerPrefs.SetFloat("MenehuneY", transform.position.y);
        PlayerPrefs.SetString("CurrentEnemy", enemyDataName);
        PlayerPrefs.SetString("ReturnScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsMenehuneLeader", isMenehuneLeader ? 1 : 0);
        PlayerPrefs.SetInt("ItemsConsumed", 0);
        PlayerPrefs.SetInt("PlayerRan", 0);
        PlayerPrefs.SetInt("ReturningFromBattle", 1);

        SaveInventoryToPrefs();
        PlayerPrefs.Save();

        StartCoroutine(LoadBattleAdditive());
    }

    private IEnumerator LoadBattleAdditive()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        PauseController.SetPause(true);

        AsyncOperation load = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        yield return load;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(battleSceneName));
    }

    public void ResetTrigger()
    {
        battleStarted = false;
    }

    private void SaveInventoryToPrefs()
    {
        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        HotbarController    hotbar    = FindFirstObjectByType<HotbarController>();
        ItemDictionary      dict      = FindFirstObjectByType<ItemDictionary>();
        if (dict == null) return;

        List<InventorySaveData> invItems    = inventory?.GetInventoryItems();
        List<InventorySaveData> hotbarItems = hotbar?.GetHotbarItems();

        SaveSlotDataToPrefs(invItems,    dict, "BattleInventoryItems");
        SaveSlotDataToPrefs(hotbarItems, dict, "BattleHotbarItems");
        SaveCombinedToPrefs(invItems, hotbarItems, dict);
    }

    private void SaveSlotDataToPrefs(List<InventorySaveData> slots, ItemDictionary dict, string key)
    {
        BattleInventoryData data = new BattleInventoryData
        {
            entries = new List<BattleInventoryUI.BattleItemEntry>()
        };

        if (slots != null)
        {
            foreach (InventorySaveData slot in slots)
            {
                GameObject prefab = dict.GetItemPrefab(slot.itemID);
                if (prefab == null) continue;
                Item item = prefab.GetComponent<Item>();
                if (item == null) continue;

                data.entries.Add(new BattleInventoryUI.BattleItemEntry
                {
                    itemName  = item.Name,
                    itemID    = item.ID,
                    quantity  = 1,
                    slotIndex = slot.slotIndex
                });
            }
        }

        PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
    }

    private void SaveCombinedToPrefs(List<InventorySaveData> invSlots,
        List<InventorySaveData> hotbarSlots, ItemDictionary dict)
    {
        Dictionary<string, BattleInventoryUI.BattleItemEntry> counts =
            new Dictionary<string, BattleInventoryUI.BattleItemEntry>();

        List<InventorySaveData> all = new List<InventorySaveData>();
        if (invSlots    != null) all.AddRange(invSlots);
        if (hotbarSlots != null) all.AddRange(hotbarSlots);

        foreach (InventorySaveData slot in all)
        {
            GameObject prefab = dict.GetItemPrefab(slot.itemID);
            if (prefab == null) continue;
            Item item = prefab.GetComponent<Item>();
            if (item == null) continue;

            if (counts.ContainsKey(item.Name))
                counts[item.Name].quantity++;
            else
                counts[item.Name] = new BattleInventoryUI.BattleItemEntry
                {
                    itemName = item.Name,
                    itemID   = item.ID,
                    quantity = 1
                };
        }

        BattleInventoryData data = new BattleInventoryData
        {
            entries = new List<BattleInventoryUI.BattleItemEntry>(counts.Values)
        };
        PlayerPrefs.SetString("BattleInventory", JsonUtility.ToJson(data));
    }
}
