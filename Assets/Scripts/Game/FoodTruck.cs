using UnityEngine;
using System.Collections;

public class FoodTruck : MonoBehaviour, IInteractable
{
    [Header("Inventory")]
    [SerializeField] private int bentoItemID = 1;
    [SerializeField] private int maxBentosPerVisit = 1;

    [Header("Cooldown")]
    [SerializeField] private float cooldownSeconds = 30f;
    private bool onCooldown = false;

    [Header("Dialogue")]
    [SerializeField] private NPC npcComponent;
    [SerializeField] private NPCDialogue giveDialogue;
    [SerializeField] private NPCDialogue cooldownDialogue;

    // Allow interact while dialogue is active so E can advance lines
    public bool CanInteract() => true;

    public void Interact()
    {
        // If dialogue is active forward E press to NPC to advance lines
        if (npcComponent != null && !npcComponent.CanInteract())
        {
            npcComponent.Interact();
            return;
        }

        if (onCooldown)
        {
            npcComponent.dialogueData = cooldownDialogue;
            npcComponent.Interact();
            return;
        }

        npcComponent.dialogueData = giveDialogue;
        npcComponent.Interact();
        StartCoroutine(WaitAndGiveBento());
    }

    private IEnumerator WaitAndGiveBento()
    {
        onCooldown = true;

        yield return new WaitUntil(() => npcComponent.CanInteract());

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory != null)
        {
            for (int i = 0; i < maxBentosPerVisit; i++)
                inventory.AddItemByID(bentoItemID);

            SoundEffectManager.Play("Chest");
        }

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ShowTutorial();

        yield return new WaitForSeconds(cooldownSeconds);
        onCooldown = false;
    }
}
