using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ToBeContinued : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI toBeContinuedText;
    [SerializeField] private float delayBeforeText = 1.5f;
    [SerializeField] private float displayDuration  = 3f;
    [SerializeField] private float fadeDuration     = 1f;
    [SerializeField] private string nextScene       = "TitleScreen";

    private void Start()
    {
        toBeContinuedText.alpha = 0f;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(delayBeforeText);

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            toBeContinuedText.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            toBeContinuedText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}
