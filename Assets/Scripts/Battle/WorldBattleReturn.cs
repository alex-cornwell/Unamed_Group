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
        RestorePlayerPosition();

        PlayerPrefs.SetInt("PlayerRan", 0);

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

        yield return null;
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

        string invJson  = PlayerPrefs.GetString("BattleInventoryItems", "");
        string hotJson  = PlayerPrefs.GetString("BattleHotbarItems", "");

        BattleInventoryData invData = JsonUtility.FromJson<BattleInventoryData>(invJson);
        BattleInventoryData hotData = JsonUtility.FromJson<BattleInventoryData>(hotJson);

        // Build original combined counts
        Dictionary<int, int> originalCombined = new Dictionary<int, int>();
        if (invData?.entries != null)
            foreach (var e in invData.entries)
                originalCombined[e.itemID] = originalCombined.GetValueOrDefault(e.itemID, 0) + 1;
        if (hotData?.entries != null)
            foreach (var e in hotData.entries)
                originalCombined[e.itemID] = originalCombined.GetValueOrDefault(e.itemID, 0) + 1;

        // Build remaining counts after battle
        Dictionary<int, int> remainingCombined = new Dictionary<int, int>();
        if (battleData.entries != null)
            foreach (var e in battleData.entries)
                remainingCombined[e.itemID] = e.quantity;

        // consumed = original - remaining
        Dictionary<int, int> consumed = new Dictionary<int, int>();
        foreach (var kv in originalCombined)
        {
            int id  = kv.Key;
            int rem = remainingCombined.GetValueOrDefault(id, 0);
            consumed[id] = kv.Value - rem;
        }

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        HotbarController    hotbar    = FindFirstObjectByType<HotbarController>();
        if (inventory == null) { ClearBattlePrefs(); return; }

        // Remove from inventory first
        List<InventorySaveData> originalInv  = GetOriginalSlots("BattleInventoryItems");
        Dictionary<int, int>   consumedCopy  = new Dictionary<int, int>(consumed);
        List<InventorySaveData> newInv        = RemoveConsumedCount(originalInv, consumedCopy);
        inventory.SetInventoryItems(newInv);

        // Reduce consumed by what was actually removed from inventory
        foreach (var kv in consumed)
        {
            int id             = kv.Key;
            int removedFromInv = originalInv.FindAll(s => s.itemID == id).Count
                               - newInv.FindAll(s => s.itemID == id).Count;
            consumedCopy[id]   = Mathf.Max(0, kv.Value - removedFromInv);
        }

        // Remove remainder from hotbar
        if (hotbar != null)
        {
            List<InventorySaveData> originalHotbar = GetOriginalSlots("BattleHotbarItems");
            List<InventorySaveData> newHotbar      = RemoveConsumedCount(originalHotbar, consumedCopy);
            hotbar.SetHotbarItems(newHotbar);
        }

        ClearBattlePrefs();
    }

    private List<InventorySaveData> RemoveConsumedCount(List<InventorySaveData> original,
        Dictionary<int, int> consumed)
    {
        Dictionary<int, int> toRemove = new Dictionary<int, int>(consumed);
        List<InventorySaveData> result = new List<InventorySaveData>();

        foreach (InventorySaveData slot in original)
        {
            int id          = slot.itemID;
            int removeCount = toRemove.GetValueOrDefault(id, 0);

            if (removeCount > 0)
                toRemove[id] = removeCount - 1; // consume — skip slot
            else
                result.Add(new InventorySaveData { itemID = id, slotIndex = slot.slotIndex });
        }

        return result;
    }

    private List<InventorySaveData> GetOriginalSlots(string key)
    {
        string json = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(json)) return new List<InventorySaveData>();

        BattleInventoryData data = JsonUtility.FromJson<BattleInventoryData>(json);
        if (data?.entries == null) return new List<InventorySaveData>();

        List<InventorySaveData> slots = new List<InventorySaveData>();
        foreach (var entry in data.entries)
            slots.Add(new InventorySaveData { itemID = entry.itemID, slotIndex = entry.slotIndex });

        return slots;
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
