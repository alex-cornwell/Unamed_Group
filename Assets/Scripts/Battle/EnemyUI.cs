using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image           enemySprite;
    [SerializeField] private Animator        enemyAnimator;
    [SerializeField] private TextMeshProUGUI nameplate;
    [SerializeField] private Slider          hpSlider;
    [SerializeField] private Image           hpFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("HP Bar Colors")]
    [SerializeField] private Color hpHighColor = new Color(0.13f, 0.8f,  0.13f);
    [SerializeField] private Color hpMidColor  = new Color(1f,    0.67f, 0f);
    [SerializeField] private Color hpLowColor  = new Color(0.88f, 0.19f, 0.19f);

    [Header("Hit Animation")]
    [SerializeField] private float shakeAmount    = 8f;
    [SerializeField] private float shakeDuration  = 0.3f;
    [SerializeField] private float flashDuration  = 0.08f;
    [SerializeField] private Image whiteFlashOverlay; // child Image over enemy sprite, same size, color (1,1,1,0), Raycast Target OFF

    [Header("HP Bar Drain")]
    [SerializeField] private float hpDrainSpeed = 2f; // units per second (slider 0-1)

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   hitSound;

    private Vector3 _originalPos;
    private float   _targetHP = 1f;
    private float   _displayHP = 1f;
    private int     _maxHP;
    private Coroutine _flashCoroutine;

    public void Initialize(EnemyData data, int currentHP)
    {
        _originalPos = enemySprite.rectTransform.anchoredPosition;
        _maxHP       = data.maxHP;
        _targetHP    = 1f;
        _displayHP   = 1f;

        nameplate.text = $"{data.enemyName.ToUpper()}  LV {data.level}";

        if (enemyAnimator != null && data.battleAnimator != null)
            enemyAnimator.runtimeAnimatorController = data.battleAnimator;
        else if (data.enemySprite != null)
            enemySprite.sprite = data.enemySprite;

        UpdateHP(currentHP, data.maxHP);
    }

    private void Update()
    {
        // Smoothly drain the displayed HP bar toward the target
        if (!Mathf.Approximately(_displayHP, _targetHP))
        {
            _displayHP = Mathf.MoveTowards(_displayHP, _targetHP, hpDrainSpeed * Time.deltaTime);
            hpSlider.value = _displayHP;
        }
    }

    public void UpdateHP(int current, int max, int damageTaken = 0)
    {
        float pct = (float)current / max;
        _targetHP     = pct;
        hpText.text   = $"HP {current} / {max}";

        if      (pct > 0.5f)  hpFill.color = hpHighColor;
        else if (pct > 0.25f) hpFill.color = hpMidColor;
        else                  hpFill.color = hpLowColor;

        if (damageTaken > 0)
        {
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(WhiteFlash());
            StartCoroutine(ShakeSprite());
        }
    }

    public void PlayDeathAnimation(bool spared)
    {
        StartCoroutine(spared ? SpareAnimation() : DefeatAnimation());
    }

    private IEnumerator WhiteFlash()
    {
        if (whiteFlashOverlay == null) yield break;

        // Snap to fully opaque white
        whiteFlashOverlay.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(flashDuration);

        // Fade out
        float elapsed = 0f;
        float fadeback = 0.15f;
        while (elapsed < fadeback)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fadeback);
            whiteFlashOverlay.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        whiteFlashOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    private IEnumerator ShakeSprite()
    {
        RectTransform rt = enemySprite.rectTransform;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeAmount, shakeAmount);
            float y = Random.Range(-shakeAmount * 0.5f, shakeAmount * 0.5f);
            rt.anchoredPosition = _originalPos + new Vector3(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = _originalPos;
    }

    private IEnumerator DefeatAnimation()
    {
        if (enemyAnimator != null) enemyAnimator.enabled = false;
        enemySprite.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            enemySprite.color = Color.Lerp(Color.white, Color.clear, t);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private IEnumerator SpareAnimation()
    {
        if (enemyAnimator != null) enemyAnimator.enabled = false;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.8f;
            enemySprite.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
