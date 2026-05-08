using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton. Attach to a persistent GameObject in BattleScene.
/// Assign a full-screen Image (RedFlashOverlay) in the Canvas — set color to
/// (1, 0, 0, 0) by default (transparent red), Raycast Target OFF.
/// </summary>
public class ScreenEffects : MonoBehaviour
{
    public static ScreenEffects Instance { get; private set; }

    [Header("Red Flash")]
    [SerializeField] private Image redFlashOverlay;
    [SerializeField] private float flashInDuration  = 0.05f;
    [SerializeField] private float flashOutDuration = 0.18f;
    [SerializeField] private float maxFlashAlpha    = 0.45f;

    [Header("Camera Shake")]
    [SerializeField] private Camera battleCamera;
    [SerializeField] private float  shakeDuration  = 0.25f;
    [SerializeField] private float  shakeMagnitude = 6f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   playerHitSound;

    private Vector3 _camOriginalPos;
    private Coroutine _shakeCoroutine;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (battleCamera != null)
            _camOriginalPos = battleCamera.transform.localPosition;

        if (redFlashOverlay != null)
            redFlashOverlay.color = new Color(1f, 0f, 0f, 0f);
    }

    /// <summary>Call this when the player takes damage.</summary>
    public void PlayerHit()
    {
        if (playerHitSound != null && audioSource != null)
            audioSource.PlayOneShot(playerHitSound);

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(RedFlash());

        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(CameraShake());
    }

    private IEnumerator RedFlash()
    {
        if (redFlashOverlay == null) yield break;

        // Flash in
        float elapsed = 0f;
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(0f, maxFlashAlpha, elapsed / flashInDuration);
            redFlashOverlay.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }

        // Flash out
        elapsed = 0f;
        while (elapsed < flashOutDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(maxFlashAlpha, 0f, elapsed / flashOutDuration);
            redFlashOverlay.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }

        redFlashOverlay.color = new Color(1f, 0f, 0f, 0f);
    }

    private IEnumerator CameraShake()
    {
        if (battleCamera == null) yield break;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float strength = Mathf.Lerp(shakeMagnitude, 0f, elapsed / shakeDuration);
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            battleCamera.transform.localPosition = _camOriginalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        battleCamera.transform.localPosition = _camOriginalPos;
    }
}
