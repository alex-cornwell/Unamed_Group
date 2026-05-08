using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Add to every EventSystem in every scene.
/// Destroys this one if another already exists (e.g. carried from a previous scene).
/// </summary>
public class EventSystemBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        EventSystem[] all = FindObjectsByType<EventSystem>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (all.Length > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }
}
