using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image enemySprite;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI damagePopup;

    [Header("HP Bar Colors")]
    [SerializeField] private Color hpHighColor = new Color(0.88f, 0.19f, 0.19f);
    [SerializeField] private Color hpMidColor  = new Color(1f, 0.67f, 0f);
    [SerializeField] private Color hpLowColor  = new Color(0.5f, 0f, 0f);

    [Header("Settings")]
    [SerializeField] private float shakeAmount = 8f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float hpAnimSpeed = 3f;

    private Vector3 _originalPos;
    private Coroutine _hpAnim;

    public void Initialize(EnemyData data, int currentHP)
    {
        _originalPos = enemySprite.rectTransform.anchoredPosition;
        if (data.enemySprite != null) enemySprite.sprite = data.enemySprite;

        float pct = (float)currentHP / data.maxHP;
        hpSlider.value = pct;
        hpText.text = $"{currentHP} / {data.maxHP}";
        UpdateBarColor(pct);

        if (damagePopup != null) damagePopup.gameObject.SetActive(false);
    }

    public void UpdateHP(int current, int max, int damageTaken = 0)
    {
        float pct = (float)current / max;

        if (_hpAnim != null) StopCoroutine(_hpAnim);
        _hpAnim = StartCoroutine(AnimateBar(pct));
        hpText.text = $"{current} / {max}";
        UpdateBarColor(pct);

        if (damageTaken > 0 && damagePopup != null)
            StartCoroutine(ShowPopup($"-{damageTaken}", Color.white));

        StartCoroutine(ShakeSprite());
    }

    private void UpdateBarColor(float pct)
    {
        if      (pct > 0.5f)  hpFill.color = hpHighColor;
        else if (pct > 0.25f) hpFill.color = hpMidColor;
        else                  hpFill.color = hpLowColor;
    }

    private IEnumerator AnimateBar(float target)
    {
        float start = hpSlider.value;
        float elapsed = 0f;
        float duration = Mathf.Max(Mathf.Abs(start - target) / hpAnimSpeed, 0.1f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hpSlider.value = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        hpSlider.value = target;
    }

    private IEnumerator ShowPopup(string text, Color color)
    {
        damagePopup.gameObject.SetActive(true);
        damagePopup.text = text;
        damagePopup.color = color;
        Vector3 startPos = damagePopup.rectTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            damagePopup.rectTransform.anchoredPosition = startPos + Vector3.up * (50f * elapsed);
            damagePopup.color = new Color(color.r, color.g, color.b, 1f - elapsed);
            yield return null;
        }
        damagePopup.gameObject.SetActive(false);
        damagePopup.rectTransform.anchoredPosition = startPos;
    }

    private IEnumerator ShakeSprite()
    {
        RectTransform rt = enemySprite.rectTransform;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            rt.anchoredPosition = _originalPos + new Vector3(Random.Range(-shakeAmount, shakeAmount), 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = _originalPos;
    }

    public void PlayDeathAnimation(bool spared)
    {
        StartCoroutine(spared ? SpareAnim() : DefeatAnim());
    }

    private IEnumerator DefeatAnim()
    {
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

    private IEnumerator SpareAnim()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.8f;
            enemySprite.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

