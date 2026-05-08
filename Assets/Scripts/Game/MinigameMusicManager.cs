using System.Collections;
using UnityEngine;

/// <summary>
/// Layered music manager for Minigame1.
/// Add clips to the Layers array — all play simultaneously, looped.
/// Optionally mute/unmute individual layers at runtime via SetLayerActive().
/// </summary>
public class MinigameMusicManager : MonoBehaviour
{
    public static MinigameMusicManager Instance { get; private set; }

    [Header("Music Layers")]
    [SerializeField] private AudioClip[] layers;
    [SerializeField] private float       targetVolume    = 0.8f;
    [SerializeField] private float       fadeInDuration  = 1.0f;
    [SerializeField] private float       fadeOutDuration = 0.8f;

    private AudioSource[] _sources;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _sources = new AudioSource[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.clip        = layers[i];
            src.loop        = true;
            src.playOnAwake = false;
            src.volume      = 0f;
            _sources[i]     = src;
        }
    }

    private void Start()
    {
        if (layers.Length > 0)
            StartCoroutine(FadeIn());
    }

    /// <summary>Pause all layers — call when entering BattleScene.</summary>
    public void Pause()
    {
        foreach (AudioSource src in _sources)
            src.Pause();
    }

    /// <summary>Resume all layers — call when returning from BattleScene.</summary>
    public void Resume()
    {
        foreach (AudioSource src in _sources)
            src.UnPause();
    }

    /// <summary>Fade a specific layer in or out at runtime. Index matches Layers array.</summary>
    public void SetLayerActive(int index, bool active, float fadeDuration = 0.5f)
    {
        if (index < 0 || index >= _sources.Length) return;
        StartCoroutine(FadeLayer(_sources[index], active ? targetVolume : 0f, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutAll());
    }

    private IEnumerator FadeIn()
    {
        foreach (AudioSource src in _sources)
            src.Play();

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float vol = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            foreach (AudioSource src in _sources)
                src.volume = vol;
            yield return null;
        }
        foreach (AudioSource src in _sources)
            src.volume = targetVolume;
    }

    private IEnumerator FadeOutAll()
    {
        float elapsed = 0f;
        float startVol = _sources.Length > 0 ? _sources[0].volume : 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float vol = Mathf.Lerp(startVol, 0f, elapsed / fadeOutDuration);
            foreach (AudioSource src in _sources)
                src.volume = vol;
            yield return null;
        }
        foreach (AudioSource src in _sources)
            src.Stop();
    }

    private IEnumerator FadeLayer(AudioSource src, float toVolume, float duration)
    {
        float elapsed  = 0f;
        float startVol = src.volume;
        while (elapsed < duration)
        {
            elapsed   += Time.deltaTime;
            src.volume = Mathf.Lerp(startVol, toVolume, elapsed / duration);
            yield return null;
        }
        src.volume = toVolume;
    }
}
