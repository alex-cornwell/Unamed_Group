using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform playerIcon;
    [SerializeField] private RectTransform mapImage;
    [SerializeField] private Transform     player;
    [SerializeField] private GameObject    mapPage; // the MapPage GameObject

    [Header("World Bounds (match your MapBounds PolygonCollider2D)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-20f, -15f);
    [SerializeField] private Vector2 worldMax = new Vector2( 60f,  15f);

    private bool wasMapOpen = false;

    private void Update()
    {
        if (mapPage == null) return;

        bool isMapOpen = mapPage.activeInHierarchy;

        // Update every frame while map is open
        if (isMapOpen)
            UpdatePlayerIcon();

        wasMapOpen = isMapOpen;
    }

    // Also call this from TabController or MenuController when map tab is clicked
    public void OnMapOpened()
    {
        UpdatePlayerIcon();
    }

    private void UpdatePlayerIcon()
    {
        if (player == null || playerIcon == null || mapImage == null) return;

        // Force layout update so rect size is correct
        Canvas.ForceUpdateCanvases();

        float normX = Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x);
        float normY = Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.y);

        float mapW = mapImage.rect.width;
        float mapH = mapImage.rect.height;

        playerIcon.anchoredPosition = new Vector2(
            (normX * mapW) - mapW * 0.5f,
            (normY * mapH) - mapH * 0.5f
        );
    }

    public void SetWorldBounds(Vector2 min, Vector2 max)
    {
        worldMin = min;
        worldMax = max;
    }
}
