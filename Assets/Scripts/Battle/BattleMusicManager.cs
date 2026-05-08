using System.Collections;
using UnityEngine;

/// <summary>
/// Self-contained battle music manager. Lives only in BattleScene.
/// Fades in on start, fades out when battle ends.
/// Call FadeOut() before unloading the scene.
/// </summary>
public class BattleMusicManager : MonoBehaviour
{
    public static BattleMusicManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip battleTrack;
    [SerializeField] private float     targetVolume  = 0.8f;
    [SerializeField] private float     fadeInDuration  = 1.0f;
    [SerializeField] private float     fadeOutDuration = 0.8f;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _source = gameObject.AddComponent<AudioSource>();
        _source.clip        = battleTrack;
        _source.loop        = true;
        _source.playOnAwake = false;
        _source.volume      = 0f;
    }

    private void Start()
    {
        if (battleTrack != null)
            StartCoroutine(FadeIn());
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeIn()
    {
        _source.Play();
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            _source.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }
        _source.volume = targetVolume;
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        float startVol = _source.volume;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _source.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        _source.Stop();
    }
}
