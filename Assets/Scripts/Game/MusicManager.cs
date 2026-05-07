using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip[] layers;
        public float[] volumes;
    }

    [Header("Scene Music")]
    [SerializeField] private SceneMusic[] sceneMusics;
    [SerializeField] private SceneMusic battleMusic;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;

    private List<AudioSource> activeSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            PlaySceneMusic(battleMusic);
            return;
        }

        foreach (SceneMusic sm in sceneMusics)
        {
            if (sm.sceneName == scene.name)
            {
                PlaySceneMusic(sm);
                return;
            }
        }
    }

    public void PlaySceneMusic(SceneMusic sm)
    {
        if (sm == null || sm.layers == null) return;
        StartCoroutine(FadeToSceneMusic(sm));
    }

    private IEnumerator FadeToSceneMusic(SceneMusic sm)
    {
        float startVol    = activeSources.Count > 0 ? activeSources[0].volume : 0f;
        float elapsed     = 0f;
        float fadeDuration = 1f / fadeSpeed;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            foreach (var src in activeSources)
                if (src != null) src.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        foreach (var src in activeSources)
            if (src != null) Destroy(src);
        activeSources.Clear();

        for (int i = 0; i < sm.layers.Length; i++)
        {
            if (sm.layers[i] == null) continue;
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.clip        = sm.layers[i];
            src.loop        = true;
            src.playOnAwake = false;
            src.volume      = 0f;
            src.Play();
            activeSources.Add(src);
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            for (int i = 0; i < activeSources.Count; i++)
            {
                if (activeSources[i] == null) continue;
                float targetVol = (sm.volumes != null && i < sm.volumes.Length)
                    ? sm.volumes[i] : 1f;
                activeSources[i].volume = Mathf.Lerp(0f, targetVol, t);
            }
            yield return null;
        }
    }

    public void SetLayerVolume(int layerIndex, float volume)
    {
        if (layerIndex < activeSources.Count && activeSources[layerIndex] != null)
            activeSources[layerIndex].volume = volume;
    }

    public void ReturnToWorldMusic()
    {
        foreach (SceneMusic sm in sceneMusics)
        {
            if (sm.sceneName == "Minigame1")
            {
                PlaySceneMusic(sm);
                return;
            }
        }
    }
}
