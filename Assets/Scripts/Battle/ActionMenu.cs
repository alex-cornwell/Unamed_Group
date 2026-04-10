using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionMenu : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button fightButton;
    [SerializeField] private Button actButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button mercyButton;

    [Header("ACT Submenu")]
    [SerializeField] private GameObject actPanel;
    [SerializeField] private Transform actButtonParent;
    [SerializeField] private GameObject actButtonPrefab;
    [SerializeField] private Button actBackButton;

    [Header("ITEM Submenu")]
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private Button spiderDonutButton;
    [SerializeField] private Button butterscotchPieButton;
    [SerializeField] private Button itemBackButton;

    [Header("Mercy Button Color")]
    [SerializeField] private TextMeshProUGUI mercyButtonLabel;
    [SerializeField] private Color canSpareColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private EnemyData _enemyData;

    private void Awake()
    {
        fightButton.onClick.AddListener(()  => BattleManager.Instance.PlayerFight());
        actButton.onClick.AddListener(()    => ShowActMenu());
        itemButton.onClick.AddListener(()   => ShowItemMenu());
        mercyButton.onClick.AddListener(()  => BattleManager.Instance.PlayerMercy());

        actBackButton.onClick.AddListener(ShowMainMenu);
        itemBackButton.onClick.AddListener(ShowMainMenu);

        spiderDonutButton.onClick.AddListener(() =>
            BattleManager.Instance.PlayerItem(12, "* You eat a Spider Donut.\n* Restored 12 HP!"));

        butterscotchPieButton.onClick.AddListener(() =>
            BattleManager.Instance.PlayerItem(99, "* You eat Toriel's Butterscotch Pie.\n* HP fully restored!"));

        BattleManager.Instance.OnMercyChanged += UpdateMercyColor;
    }

    public void Initialize(EnemyData data)
    {
        _enemyData = data;
        BuildActButtons();
    }

    private void BuildActButtons()
    {
        // Clear old buttons
        foreach (Transform child in actButtonParent)
            Destroy(child.gameObject);

        foreach (var act in _enemyData.actOptions)
        {
            var actCopy = act; // capture for lambda
            GameObject go = Instantiate(actButtonPrefab, actButtonParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = actCopy.actName;

            string dialogue = actCopy.actName == "Check"
                ? _enemyData.checkDialogue
                : actCopy.dialogue;

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowMainMenu();
                BattleManager.Instance.PlayerAct(actCopy.actName, actCopy.mercyGain, dialogue);
            });
        }
    }

    // -------------------------------------------------------------------------
    // Visibility
    // -------------------------------------------------------------------------

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        actPanel.SetActive(false);
        itemPanel.SetActive(false);
    }

    public void ShowActMenu()
    {
        mainMenuPanel.SetActive(false);
        actPanel.SetActive(true);
        itemPanel.SetActive(false);
    }

    public void ShowItemMenu()
    {
        mainMenuPanel.SetActive(false);
        actPanel.SetActive(false);
        itemPanel.SetActive(true);
    }

    public void HideAll()
    {
        mainMenuPanel.SetActive(false);
        actPanel.SetActive(false);
        itemPanel.SetActive(false);
    }

    private void UpdateMercyColor(int mercyPercent)
    {
        if (mercyButtonLabel != null)
            mercyButtonLabel.color = mercyPercent >= 100 ? canSpareColor : normalColor;
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnMercyChanged -= UpdateMercyColor;
    }
}
