using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;

    private void Awake()
    {
        InitializeComponents();

        if (PlayerPrefs.GetInt("ReturningFromBattle", 0) == 1)
        {
            PlayerPrefs.SetInt("ReturningFromBattle", 0);
            PlayerPrefs.Save();
            return;
        }

        // Clear stale battle prefs on fresh start
        PlayerPrefs.DeleteKey("BattleInventory");
        PlayerPrefs.DeleteKey("BattleInventoryItems");
        PlayerPrefs.DeleteKey("BattleHotbarItems");
        PlayerPrefs.DeleteKey("ItemsConsumed");
        PlayerPrefs.DeleteKey("PlayerReturnX");
        PlayerPrefs.DeleteKey("PlayerReturnY");
        PlayerPrefs.DeleteKey("HasReturnPosition");
        PlayerPrefs.DeleteKey("MenehuneX");
        PlayerPrefs.DeleteKey("MenehuneY");

        LoadGame();
    }

    void Start() { } // intentionally empty — load happens in Awake

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindFirstObjectByType<InventoryController>();
        hotbarController    = FindFirstObjectByType<HotbarController>();
        chests              = FindObjectsByType<Chest>(FindObjectsSortMode.None);
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || inventoryController == null || hotbarController == null) return;

        SaveData saveData = new SaveData
        {
            playerPosition    = player.transform.position,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData    = hotbarController.GetHotbarItems(),
            chestSaveData     = GetChestsState()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        foreach (Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData
            {
                chestID  = chest.ChestID,
                isOpened = chest.IsOpened
            });
        }
        return chestStates;
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = saveData.playerPosition;

            inventoryController?.SetInventoryItems(
                saveData.inventorySaveData ?? new List<InventorySaveData>());
            hotbarController?.SetHotbarItems(
                saveData.hotbarSaveData ?? new List<InventorySaveData>());

            if (saveData.chestSaveData != null)
                LoadChestStates(saveData.chestSaveData);
        }
        else
        {
            inventoryController?.SetInventoryItems(new List<InventorySaveData>());
            hotbarController?.SetHotbarItems(new List<InventorySaveData>());
        }
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData data = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);
            if (data != null)
                chest.SetOpened(data.isOpened);
        }
    }

    [ContextMenu("Clear Save Data")]
    public void ClearSaveData()
    {
        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Save data cleared");
        }
    }
}
