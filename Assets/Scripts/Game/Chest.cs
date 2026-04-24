using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool IsOpened { get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;
    public Sprite openedSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject); //Unique ID
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        public bool CanInteract()
    {
        return !IsOpened; // Can interact if the chest is not already opened
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpened(true);
        SoundEffectManager.Play("Chest"); //play chest opening sound effect
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity); //spawn the item at the chest position
            droppedItem.GetComponent<BounceEffect>().StartBounce(); //add bounce effect to the dropped item
        }
    }

    public void SetOpened(bool opened)
    {
        if (IsOpened = opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite; //change chest sprite to opened version
        }
    }
}
