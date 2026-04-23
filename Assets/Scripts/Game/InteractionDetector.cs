using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false); //hide the interaction icon at the start
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableInRange?.Interact(); //call the Interact method on the interactable object if it's in range
            if(!interactableInRange.CanInteract())
            {
                interactionIcon.SetActive(false); //hide the interaction icon
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable; //store the interactable object in range
            interactionIcon.SetActive(true); //show the interaction icon
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null; //clear the interactable object when we leave the range
            interactionIcon.SetActive(false); //hide the interaction icon
        }
    }
}
