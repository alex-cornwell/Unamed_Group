using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI stepCounter;

    [Header("Step Icons (optional)")]
    [SerializeField] private Sprite inventoryIcon;
    [SerializeField] private Sprite hotbarIcon;
    [SerializeField] private Sprite bentoIcon;
    [SerializeField] private Sprite foodTruckIcon;

    private TutorialStep[] steps;
    private int currentStep = 0;

    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        [TextArea(2, 4)]
        public string body;
        public Sprite icon;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorial()
    {
        // Only show once — skip if already seen
        if (PlayerPrefs.GetInt("TutorialShown", 0) == 1) return;

        steps = new TutorialStep[]
        {
            new TutorialStep
            {
                title = "Inventory",
                body  = "You got a bento!\nOpen your bag with TAB to see your items.",
                icon  = inventoryIcon
            },
            new TutorialStep
            {
                title = "Hotbar",
                body  = "Drag items from your inventory down to the hotbar.\nPress 1-5 to quickly select hotbar items.",
                icon  = hotbarIcon
            },
            new TutorialStep
            {
                title = "Drop Bento",
                body  = "Drag a bento from your hotbar onto the ground to drop it.\nNearby Menehune will smell it and come investigate!",
                icon  = bentoIcon
            },
            new TutorialStep
            {
                title = "Food Truck",
                body  = "Come back to the food truck after a while for more free bento.\nThey give them out late at night!",
                icon  = foodTruckIcon
            }
        };

        currentStep = 0;
        tutorialPanel.SetActive(true);
        PauseController.SetPause(true);
        DisplayStep(currentStep);
    }

    private void DisplayStep(int index)
    {
        TutorialStep step = steps[index];
        titleText.text = step.title;
        bodyText.text  = step.body;

        if (iconImage != null)
        {
            iconImage.sprite  = step.icon;
            iconImage.enabled = step.icon != null;
        }

        if (stepCounter != null)
            stepCounter.text = $"{index + 1} / {steps.Length}";

        // Show Next on all steps except the last
        if (nextButton != null)
            nextButton.gameObject.SetActive(index < steps.Length - 1);

        // Show Close only on the last step
        if (closeButton != null)
            closeButton.gameObject.SetActive(index == steps.Length - 1);
    }

    public void NextStep()
    {
        if (currentStep < steps.Length - 1)
        {
            currentStep++;
            DisplayStep(currentStep);
        }
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        PauseController.SetPause(false);
        PlayerPrefs.SetInt("TutorialShown", 1);
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Tutorial")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("TutorialShown");
    }
}
