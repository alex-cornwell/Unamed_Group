using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

public class BattleEventSystemController : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null; // wait one frame for DontDestroyOnLoad objects to settle

        // Check if a persistent EventSystem already exists from TitleScreen
        UnityEngine.EventSystems.EventSystem[] systems =
            FindObjectsByType<UnityEngine.EventSystems.EventSystem>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (systems.Length > 1)
        {
            // TitleScreen EventSystem is already here — disable this one
            gameObject.SetActive(false);
        }
        else
        {
            // Starting directly from Minigame1 — wire UI Input Module to Player
            InputSystemUIInputModule uiModule =
                GetComponent<InputSystemUIInputModule>();
            UnityEngine.InputSystem.PlayerInput playerInput =
                FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();

            if (playerInput != null && uiModule != null)
                playerInput.uiInputModule = uiModule;
        }
    }
}