using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpLabel;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI damagePopup;

    [Header("Colors")]
    [SerializeField] private Color hpFullColor = Color.yellow;
    [SerializeField] private Color hpLowColor  = new Color(0.88f, 0.19f, 0.19f);

    [Header("Settings")]
    [SerializeField] private float lowHPPercent = 0.3f;
    [SerializeField] private float hpAnimSpeed = 3f;

    private Coroutine _hpAnim;

    public void Initialize(int maxHP, int currentHP)
    {
        float pct = (float)currentHP / maxHP;
        hpSlider.value = pct;
        hpLabel.text = $"HP  {currentHP} / {maxHP}";
        hpFill.color = pct > lowHPPercent ? hpFullColor : hpLowColor;
        if (damagePopup != null) damagePopup.gameObject.SetActive(false);
    }

    public void UpdateHP(int current, int max, int damageTaken = 0, int healAmount = 0)
    {
        float pct = (float)current / max;

        if (_hpAnim != null) StopCoroutine(_hpAnim);
        _hpAnim = StartCoroutine(AnimateBar(pct));

        hpLabel.text = $"HP  {current} / {max}";
        hpFill.color = pct > lowHPPercent ? hpFullColor : hpLowColor;

        if (damageTaken > 0 && damagePopup != null)
            StartCoroutine(ShowPopup($"-{damageTaken}", Color.red));
        else if (healAmount > 0 && damagePopup != null)
            StartCoroutine(ShowPopup($"+{healAmount}", Color.green));

        if (pct <= lowHPPercent)
            StartCoroutine(PulseHP());
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

    private IEnumerator PulseHP()
    {
        float t = 0f;
        Color start = hpFill.color;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            hpFill.color = Color.Lerp(start, Color.white, Mathf.PingPong(t * 2f, 1f) * 0.4f);
            yield return null;
        }
        hpFill.color = start;
    }
}
