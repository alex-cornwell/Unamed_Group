using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBattleReturn : MonoBehaviour
{
    [SerializeField] private GameObject driveBeltWorldPrefab;

    public void OnBattleReturn(bool battleWon)
    {
        StartCoroutine(HandleReturn(battleWon));
    }

    public IEnumerator HandleReturn(bool battleWon)
    {
        yield return null;

        RestorePlayerPosition();

        if (battleWon)
        {
            DestroyBattledMenehune();

            int dropBelt = PlayerPrefs.GetInt("DropDriveBelt", 0);
            if (dropBelt == 1)
            {
                PlayerPrefs.SetInt("DropDriveBelt", 0);
                PlayerPrefs.Save();
                DropDriveBelt();
            }
        }
        else
        {
            PushMenehuneAway();
        }

        SyncInventoryFromBattle();
    }

    private void RestorePlayerPosition()
    {
        if (PlayerPrefs.GetInt("HasReturnPosition", 0) == 0) return;

        float x = PlayerPrefs.GetFloat("PlayerReturnX", 0f);
        float y = PlayerPrefs.GetFloat("PlayerReturnY", 0f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            player.transform.position = new Vector3(x, y, 0f);
        }

        PlayerPrefs.DeleteKey("PlayerReturnX");
        PlayerPrefs.DeleteKey("PlayerReturnY");
        PlayerPrefs.DeleteKey("HasReturnPosition");
    }

    private void DestroyBattledMenehune()
    {
        if (!PlayerPrefs.HasKey("MenehuneX")) return;

        float mx = PlayerPrefs.GetFloat("MenehuneX");
        float my = PlayerPrefs.GetFloat("MenehuneY");

        Menehune[] menehunes = FindObjectsByType<Menehune>(FindObjectsSortMode.None);
        foreach (Menehune m in menehunes)
        {
            if (Vector2.Distance(m.transform.position, new Vector2(mx, my)) < 2f)
            {
                Destroy(m.gameObject);
                break;
            }
        }

        PlayerPrefs.DeleteKey("MenehuneX");
        PlayerPrefs.DeleteKey("MenehuneY");
    }

    private void PushMenehuneAway()
    {
        if (!PlayerPrefs.HasKey("MenehuneX")) return;

        float mx = PlayerPrefs.GetFloat("MenehuneX");
        float my = PlayerPrefs.GetFloat("MenehuneY");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Menehune[] menehunes = FindObjectsByType<Menehune>(FindObjectsSortMode.None);
        foreach (Menehune m in menehunes)
        {
            if (Vector2.Distance(m.transform.position, new Vector2(mx, my)) < 2f)
            {
                Vector2 pushDir = ((Vector2)m.transform.position - (Vector2)player.transform.position).normalized;
                if (pushDir == Vector2.zero) pushDir = Vector2.right;
                m.transform.position = (Vector2)player.transform.position + pushDir * 4f;

                BattleTrigger bt = m.GetComponent<BattleTrigger>();
                if (bt != null) bt.ResetTrigger();
                break;
            }
        }

        PlayerPrefs.DeleteKey("MenehuneX");
        PlayerPrefs.DeleteKey("MenehuneY");
    }

    private void DropDriveBelt()
    {
        if (driveBeltWorldPrefab == null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 pos = player != null
            ? player.transform.position + Vector3.right * 2f
            : Vector3.zero;
        GameObject dropped = Instantiate(driveBeltWorldPrefab, pos, Quaternion.identity);
        dropped.GetComponent<BounceEffect>()?.StartBounce();
    }

    private void SyncInventoryFromBattle()
    {
        if (PlayerPrefs.GetInt("ItemsConsumed", 0) == 0)
        {
            ClearBattlePrefs();
            return;
        }

        string battleJson = PlayerPrefs.GetString("BattleInventory", "");
        if (string.IsNullOrEmpty(battleJson)) { ClearBattlePrefs(); return; }

        BattleInventoryData battleData = JsonUtility.FromJson<BattleInventoryData>(battleJson);
        if (battleData == null) { ClearBattlePrefs(); return; }

        Dictionary<int, int> remaining = new Dictionary<int, int>();
        if (battleData.entries != null)
        {
            foreach (var entry in battleData.entries)
            {
                if (remaining.ContainsKey(entry.itemID))
                    remaining[entry.itemID] += entry.quantity;
                else
                    remaining[entry.itemID] = entry.quantity;
            }
        }

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        HotbarController hotbar       = FindFirstObjectByType<HotbarController>();
        ItemDictionary dict           = FindFirstObjectByType<ItemDictionary>();
        if (inventory == null || dict == null) { ClearBattlePrefs(); return; }

        List<InventorySaveData> originalInv = GetOriginalSlots("BattleInventoryItems");
        List<InventorySaveData> newInv = RemoveConsumed(originalInv, new Dictionary<int, int>(remaining));
        inventory.SetInventoryItems(newInv);

        if (hotbar != null)
        {
            List<InventorySaveData> originalHotbar = GetOriginalSlots("BattleHotbarItems");
            List<InventorySaveData> newHotbar = RemoveConsumed(originalHotbar, new Dictionary<int, int>(remaining));
            hotbar.SetHotbarItems(newHotbar);
        }

        ClearBattlePrefs();
    }

    private List<InventorySaveData> GetOriginalSlots(string key)
    {
        string json = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(json)) return new List<InventorySaveData>();

        BattleInventoryData data = JsonUtility.FromJson<BattleInventoryData>(json);
        if (data == null || data.entries == null) return new List<InventorySaveData>();

        List<InventorySaveData> slots = new List<InventorySaveData>();
        foreach (var entry in data.entries)
            slots.Add(new InventorySaveData { itemID = entry.itemID, slotIndex = entry.slotIndex });

        return slots;
    }

    private List<InventorySaveData> RemoveConsumed(List<InventorySaveData> original,
        Dictionary<int, int> remaining)
    {
        Dictionary<int, int> originalCounts = new Dictionary<int, int>();
        foreach (InventorySaveData slot in original)
        {
            if (originalCounts.ContainsKey(slot.itemID))
                originalCounts[slot.itemID]++;
            else
                originalCounts[slot.itemID] = 1;
        }

        Dictionary<int, int> keepCount = new Dictionary<int, int>();
        foreach (var kv in originalCounts)
        {
            int id = kv.Key;
            int originalQty = kv.Value;
            keepCount[id] = remaining.ContainsKey(id) ? remaining[id] : originalQty;
        }

        Dictionary<int, int> kept = new Dictionary<int, int>();
        List<InventorySaveData> result = new List<InventorySaveData>();
        int newSlot = 0;

        foreach (InventorySaveData slot in original)
        {
            int id = slot.itemID;
            int alreadyKept = kept.ContainsKey(id) ? kept[id] : 0;
            int maxKeep = keepCount.ContainsKey(id) ? keepCount[id] : 0;

            if (alreadyKept < maxKeep)
            {
                result.Add(new InventorySaveData { itemID = id, slotIndex = newSlot++ });
                kept[id] = alreadyKept + 1;
            }
        }

        return result;
    }

    private void ClearBattlePrefs()
    {
        PlayerPrefs.DeleteKey("BattleInventory");
        PlayerPrefs.DeleteKey("BattleInventoryItems");
        PlayerPrefs.DeleteKey("BattleHotbarItems");
        PlayerPrefs.DeleteKey("ItemsConsumed");
        PlayerPrefs.Save();
    }
}
