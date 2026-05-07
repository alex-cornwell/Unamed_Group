using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance { get; private set; }

    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InitializeComponents();

        // Always reset HP on fresh editor start regardless of battle state
        PlayerPrefs.DeleteKey("PlayerHP");

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
        PlayerPrefs.DeleteKey("TutorialShown");
        PlayerPrefs.DeleteKey("TruckToolsGiven");
        PlayerPrefs.DeleteKey("TruckFixed");

        LoadGame();
    }

    void Start() { }

    private void InitializeComponents()
    {
        saveLocation        = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindFirstObjectByType<InventoryController>();
        hotbarController    = FindFirstObjectByType<HotbarController>();
        chests              = FindObjectsByType<Chest>(FindObjectsSortMode.None);
    }

    // ── HP ───────────────────────────────────────────────────────────────────

    public void SavePlayerHP(int hp)
    {
        PlayerPrefs.SetInt("PlayerHP", hp);
        PlayerPrefs.Save();
    }

    public int LoadPlayerHP(int defaultHP = 20)
    {
        return PlayerPrefs.GetInt("PlayerHP", defaultHP);
    }

    // ── SAVE / LOAD ──────────────────────────────────────────────────────────

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || inventoryController == null || hotbarController == null) return;

        SaveData saveData = new SaveData
        {
            playerPosition    = player.transform.position,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData    = hotbarController.GetHotbarItems(),
            chestSaveData     = GetChestsState(),
            playerHP          = LoadPlayerHP(20)
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

            PlayerPrefs.SetInt("PlayerHP", saveData.playerHP > 0 ? saveData.playerHP : 20);
        }
        else
        {
            inventoryController?.SetInventoryItems(new List<InventorySaveData>());
            hotbarController?.SetHotbarItems(new List<InventorySaveData>());
            PlayerPrefs.SetInt("PlayerHP", 20);
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

    // ── TITLE SCREEN ─────────────────────────────────────────────────────────

    public void ReturnToTitleScreen()
    {
        PlayerPrefs.DeleteKey("BattleInventory");
        PlayerPrefs.DeleteKey("BattleInventoryItems");
        PlayerPrefs.DeleteKey("BattleHotbarItems");
        PlayerPrefs.DeleteKey("ItemsConsumed");
        PlayerPrefs.DeleteKey("ReturningFromBattle");
        PlayerPrefs.Save();

        SceneManager.LoadScene("TitleScreen");
    }

    [ContextMenu("Clear Save Data")]
    public void ClearSaveData()
    {
        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Save data cleared");
        }
        PlayerPrefs.DeleteKey("PlayerHP");
        PlayerPrefs.Save();
    }
}
