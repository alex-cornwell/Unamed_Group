using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIInputLinker : MonoBehaviour
{
    private void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        InputSystemUIInputModule uiModule = 
            FindFirstObjectByType<InputSystemUIInputModule>();
        
        if (playerInput != null && uiModule != null)
            playerInput.uiInputModule = uiModule;
    }
}