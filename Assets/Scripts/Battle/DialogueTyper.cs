using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueTyper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject cursorBlinker; // small blinking rectangle

    [Header("Settings")]
    [SerializeField] private float charDelay = 0.028f;
    [SerializeField] private float punctuationDelay = 0.12f;  // longer pause after . ! ?
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typeSounds;           // randomize for variety

    private Coroutine _typingCoroutine;
    private bool _skipRequested = false;

    private void Update()
    {
        // Allow player to skip / fast-forward typing with Z or Enter
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            _skipRequested = true;
    }

    // Awaitable version — use in coroutine sequences with "yield return"
    public IEnumerator TypeDialogue(string message)
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _skipRequested = false;

        bool finished = false;
        _typingCoroutine = StartCoroutine(TypeRoutine(message, () => finished = true));
        yield return new WaitUntil(() => finished);

        // Wait for player to press Z/Enter before continuing
        yield return WaitForConfirm();
    }

    // Fire-and-forget version — use when you don't need to await
    public void TypeDialogueNoWait(string message)
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _skipRequested = false;
        _typingCoroutine = StartCoroutine(TypeRoutine(message, null));
    }

    private IEnumerator TypeRoutine(string message, System.Action onComplete)
    {
        dialogueText.text = "";
        cursorBlinker?.SetActive(false);

        for (int i = 0; i < message.Length; i++)
        {
            if (_skipRequested)
            {
                dialogueText.text = message;
                break;
            }

            char c = message[i];
            dialogueText.text += c;

            PlayTypeSound(c);

            float delay = IsPunctuation(c) ? punctuationDelay : charDelay;
            yield return new WaitForSeconds(delay);
        }

        cursorBlinker?.SetActive(true);
        onComplete?.Invoke();
        _typingCoroutine = null;
    }

    private IEnumerator WaitForConfirm()
    {
        // Wait at least one frame so the keydown that triggered skip
        // doesn't immediately confirm
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return));
        cursorBlinker?.SetActive(false);
    }

    private void PlayTypeSound(char c)
    {
        if (audioSource == null || typeSounds == null || typeSounds.Length == 0) return;
        if (c == ' ' || c == '\n') return;

        AudioClip clip = typeSounds[Random.Range(0, typeSounds.Length)];
        audioSource.PlayOneShot(clip, 0.4f);
    }

    private bool IsPunctuation(char c) =>
        c == '.' || c == '!' || c == '?' || c == ',';
}
