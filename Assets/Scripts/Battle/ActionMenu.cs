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
    [SerializeField] private Button runButton;

    [Header("ACT Submenu")]
    [SerializeField] private GameObject actPanel;
    [SerializeField] private Transform actButtonParent;
    [SerializeField] private GameObject actButtonPrefab;
    [SerializeField] private Button actBackButton;

    [Header("ITEM Submenu")]
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private Button itemBackButton;

    private void Awake()
    {
        fightButton.onClick.AddListener(() => BattleManager.Instance.PlayerFight());
        actButton.onClick.AddListener(ShowActMenu);
        itemButton.onClick.AddListener(ShowItemMenu);
        runButton.onClick.AddListener(() => BattleManager.Instance.PlayerRun());
        actBackButton.onClick.AddListener(ShowMainMenu);
        itemBackButton.onClick.AddListener(ShowMainMenu);
    }

    public void Initialize(EnemyData data)
    {
        BuildActButtons(data);
    }

    private void BuildActButtons(EnemyData data)
    {
        foreach (Transform child in actButtonParent)
            Destroy(child.gameObject);

        foreach (var act in data.actOptions)
        {
            var copy = act;
            string dialogue = copy.actName == "Check" ? data.checkDialogue : copy.dialogue;

            GameObject go = Instantiate(actButtonPrefab, actButtonParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = copy.actName;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowMainMenu();
                BattleManager.Instance.PlayerAct(copy.actName, copy.mercyGain, dialogue);
            });
        }

        // Trade Bento always in ACT
        GameObject trade = Instantiate(actButtonPrefab, actButtonParent);
        trade.GetComponentInChildren<TextMeshProUGUI>().text = "Trade Bento";
        trade.GetComponent<Button>().onClick.AddListener(() =>
        {
            ShowMainMenu();
            BattleManager.Instance.PlayerTradeBento();
        });
    }

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

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
            actPanel.GetComponent<RectTransform>());
    }

    public void ShowItemMenu()
    {
        mainMenuPanel.SetActive(false);
        actPanel.SetActive(false);
        itemPanel.SetActive(true);

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
            itemPanel.GetComponent<RectTransform>());
    }

    public void HideAll()
    {
        mainMenuPanel.SetActive(false);
        actPanel.SetActive(false);
        itemPanel.SetActive(false);
    }
}
