using UnityEngine;
using System.Collections;

public class BrokenTruck : MonoBehaviour, IInteractable
{
    [Header("Items")]
    [SerializeField] private int hammerItemID    = 3;
    [SerializeField] private int drillItemID     = 4;
    [SerializeField] private int driveBeltItemID = 2;

    [Header("Dialogue")]
    [SerializeField] private NPC npcComponent;
    [SerializeField] private NPCDialogue firstVisitDialogue;
    [SerializeField] private NPCDialogue waitingDialogue;
    [SerializeField] private NPCDialogue fixedDialogue;

    private bool toolsGiven    = false;
    private bool truckFixed    = false;
    private bool isGivingTools = false;

    private void Start()
    {
        toolsGiven = PlayerPrefs.GetInt("TruckToolsGiven", 0) == 1;
        truckFixed  = PlayerPrefs.GetInt("TruckFixed", 0) == 1;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = !truckFixed;
    }

    public bool CanInteract() => !truckFixed;

    public void Interact()
    {
        if (truckFixed) return;

        // If dialogue is active forward E press to NPC to advance lines
        if (npcComponent != null && !npcComponent.CanInteract())
        {
            npcComponent.Interact();
            return;
        }

        if (!toolsGiven && !isGivingTools)
        {
            StartCoroutine(GiveTools());
        }
        else if (toolsGiven)
        {
            InventoryController inventory = FindFirstObjectByType<InventoryController>();
            HotbarController    hotbar    = FindFirstObjectByType<HotbarController>();

            bool hasDriveBelt = HasItem(inventory, driveBeltItemID) || HasItem(hotbar, driveBeltItemID);

            if (hasDriveBelt)
                StartCoroutine(FixTruck());
            else
            {
                npcComponent.dialogueData = waitingDialogue;
                npcComponent.Interact();
            }
        }
    }

    private bool HasItem(InventoryController inv, int itemID)
    {
        if (inv == null) return false;
        foreach (InventorySaveData data in inv.GetInventoryItems())
            if (data.itemID == itemID) return true;
        return false;
    }

    private bool HasItem(HotbarController hotbar, int itemID)
    {
        if (hotbar == null) return false;
        foreach (InventorySaveData data in hotbar.GetHotbarItems())
            if (data.itemID == itemID) return true;
        return false;
    }

    private IEnumerator GiveTools()
    {
        isGivingTools = true;

        npcComponent.dialogueData = firstVisitDialogue;
        npcComponent.Interact();

        float timeout = 30f;
        float elapsed = 0f;
        yield return new WaitUntil(() =>
        {
            elapsed += Time.deltaTime;
            return npcComponent.CanInteract() || elapsed >= timeout;
        });

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory != null)
        {
            inventory.AddItemByID(hammerItemID);
            inventory.AddItemByID(drillItemID);
            SoundEffectManager.Play("Chest");
        }

        toolsGiven    = true;
        isGivingTools = false;
        PlayerPrefs.SetInt("TruckToolsGiven", 1);
        PlayerPrefs.Save();
    }

    private IEnumerator FixTruck()
    {
        npcComponent.dialogueData = fixedDialogue;
        npcComponent.Interact();

        yield return new WaitUntil(() => npcComponent.CanInteract());

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        HotbarController    hotbar    = FindFirstObjectByType<HotbarController>();

        if (inventory != null && HasItem(inventory, driveBeltItemID))
            inventory.RemoveItemsFromInventory(driveBeltItemID, 1);
        else if (hotbar != null && HasItem(hotbar, driveBeltItemID))
            hotbar.RemoveItemsFromHotbar(driveBeltItemID, 1);

        truckFixed = true;
        PlayerPrefs.SetInt("TruckFixed", 1);
        PlayerPrefs.Save();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene("ToBeContinued");
    }
}
