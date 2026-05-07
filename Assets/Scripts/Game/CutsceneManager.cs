using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private GameObject skipPrompt;
    [SerializeField] private GameObject continuePrompt;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typingSounds;
    [SerializeField] private float typingPitch = 1f;

    [Header("Settings")]
    [SerializeField] private float fadeDuration   = 0.8f;
    [SerializeField] private float charDelay      = 0.03f;
    [SerializeField] private string nextSceneName = "Minigame1";

    [Header("Cutscene Data")]
    [SerializeField] private CutsceneScene[] scenes;

    [System.Serializable]
    public class CutsceneScene
    {
        public Sprite background;
        [TextArea(2, 4)]
        public string speaker;
        [TextArea(2, 6)]
        public string dialogue;
        public bool fadeInBackground  = true;
        public bool sameBgAsPrevious  = false; // skip fade if same bg as previous scene
        public float holdBeforeDialogue = 0.5f;
    }

    private int currentScene = 0;
    private bool isTyping    = false;
    private bool skipPressed = false;
    private bool canAdvance  = false;

    private void Start()
    {
        fadeOverlay.color = Color.black;
        dialogueBox.SetActive(false);
        if (skipPrompt    != null) skipPrompt.SetActive(true);
        if (continuePrompt != null) continuePrompt.SetActive(false);

        StartCoroutine(PlayCutscene());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                skipPressed = true;
            }
            else if (canAdvance)
            {
                canAdvance = false;
                currentScene++;
                if (currentScene >= scenes.Length)
                    StartCoroutine(EndCutscene());
                else
                    StartCoroutine(ShowScene(currentScene));
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            StartCoroutine(EndCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowScene(0));
    }

    private IEnumerator ShowScene(int index)
    {
        CutsceneScene scene = scenes[index];
        canAdvance = false;

        // Only fade if background is different from previous scene
        if (index == 0)
        {
            if (scene.background != null)
                backgroundImage.sprite = scene.background;
            if (scene.fadeInBackground)
                yield return StartCoroutine(Fade(1f, 0f));
        }
        else if (!scene.sameBgAsPrevious)
        {
            yield return StartCoroutine(Fade(0f, 1f));

            if (scene.background != null)
                backgroundImage.sprite = scene.background;

            if (scene.fadeInBackground)
                yield return StartCoroutine(Fade(1f, 0f));
        }

        yield return new WaitForSeconds(scene.holdBeforeDialogue);

        if (!string.IsNullOrEmpty(scene.dialogue))
        {
            dialogueBox.SetActive(true);

            if (speakerText != null)
            {
                speakerText.text = scene.speaker;
                speakerText.gameObject.SetActive(!string.IsNullOrEmpty(scene.speaker));
            }

            yield return StartCoroutine(TypeDialogue(scene.dialogue));

            if (continuePrompt != null) continuePrompt.SetActive(true);
            canAdvance = true;
        }
        else
        {
            dialogueBox.SetActive(false);
            yield return new WaitForSeconds(1.5f);
            currentScene++;
            if (currentScene >= scenes.Length)
                StartCoroutine(EndCutscene());
            else
                StartCoroutine(ShowScene(currentScene));
        }
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping    = true;
        skipPressed = false;
        dialogueText.text = "";

        if (continuePrompt != null) continuePrompt.SetActive(false);

        foreach (char c in text)
        {
            if (skipPressed)
            {
                dialogueText.text = text;
                break;
            }

            dialogueText.text += c;

            // Play typing sound on non-space characters
            if (c != ' ' && c != '\n' && typingSounds != null && typingSounds.Length > 0)
            {
                AudioClip clip = typingSounds[Random.Range(0, typingSounds.Length)];
                if (clip != null && audioSource != null)
                {
                    audioSource.pitch = typingPitch;
                    audioSource.PlayOneShot(clip, 0.4f);
                }
            }

            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color col = fadeOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            col.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeOverlay.color = col;
            yield return null;
        }

        col.a = to;
        fadeOverlay.color = col;
    }

    private IEnumerator EndCutscene()
    {
        canAdvance = false;
        dialogueBox.SetActive(false);
        if (skipPrompt    != null) skipPrompt.SetActive(false);
        if (continuePrompt != null) continuePrompt.SetActive(false);

        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(nextSceneName);
    }
}
