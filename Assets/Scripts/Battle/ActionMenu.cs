using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionMenu : MonoBehaviour
{
    [Header("Main Menu")]
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
    [SerializeField] private Button itemDonutButton;
    [SerializeField] private Button itemButterscotchButton;
    [SerializeField] private Button itemBackButton;

    [Header("Mercy")]
    [SerializeField] private TextMeshProUGUI mercyButtonLabel;
    [SerializeField] private Color canSpareColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private EnemyData _enemyData;

    private void Awake()
    {
        fightButton.onClick.AddListener(() => BattleManager.Instance.PlayerFight());
        actButton.onClick.AddListener(ShowActMenu);
        itemButton.onClick.AddListener(ShowItemMenu);
        mercyButton.onClick.AddListener(() => BattleManager.Instance.PlayerMercy());

        actBackButton.onClick.AddListener(ShowMainMenu);
        itemBackButton.onClick.AddListener(ShowMainMenu);

        // Item submenu buttons
        itemDonutButton.onClick.AddListener(() =>
            BattleManager.Instance.UseItem("Spider Donut"));
        itemButterscotchButton.onClick.AddListener(() =>
            BattleManager.Instance.UseItem("Butterscotch Pie"));

        BattleManager.Instance.OnMercyChanged += UpdateMercyColor;
    }

    public void Initialize(EnemyData data)
    {
        _enemyData = data;
        BuildActButtons();
    }

    private void BuildActButtons()
    {
        foreach (Transform child in actButtonParent)
            Destroy(child.gameObject);

        // Standard act options from EnemyData
        foreach (var act in _enemyData.actOptions)
        {
            var actCopy = act;
            string dialogue = actCopy.actName == "Check"
                ? _enemyData.checkDialogue
                : actCopy.dialogue;

            GameObject go = Instantiate(actButtonPrefab, actButtonParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = actCopy.actName;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowMainMenu();
                BattleManager.Instance.PlayerAct(actCopy.actName, actCopy.mercyGain, dialogue);
            });
        }

        // Trade Bento button — always available in ACT
        GameObject tradeBtn = Instantiate(actButtonPrefab, actButtonParent);
        tradeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Trade Bento";
        tradeBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ShowMainMenu();
            BattleManager.Instance.PlayerTradeBento();
        });
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
