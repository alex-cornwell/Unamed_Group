using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
// ENEMY UI
// ============================================================
public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image enemySprite;
    [SerializeField] private TextMeshProUGUI nameplate;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("HP Bar Colors")]
    [SerializeField] private Color hpHighColor   = new Color(0.13f, 0.8f,  0.13f); // green
    [SerializeField] private Color hpMidColor    = new Color(1f,    0.67f, 0f);    // orange
    [SerializeField] private Color hpLowColor    = new Color(0.88f, 0.19f, 0.19f); // red

    [Header("Hit Animation")]
    [SerializeField] private float shakeAmount = 8f;
    [SerializeField] private float shakeDuration = 0.3f;

    private Vector3 _originalPos;

    public void Initialize(EnemyData data, int currentHP)
    {
        _originalPos = enemySprite.rectTransform.anchoredPosition;
        nameplate.text = $"{data.enemyName.ToUpper()}  LV {data.level}";
        if (data.enemySprite != null) enemySprite.sprite = data.enemySprite;
        UpdateHP(currentHP, data.maxHP);
    }

    public void UpdateHP(int current, int max)
    {
        float pct = (float)current / max;
        hpSlider.value = pct;
        hpText.text = $"HP {current} / {max}";

        if      (pct > 0.5f) hpFill.color = hpHighColor;
        else if (pct > 0.25f) hpFill.color = hpMidColor;
        else                  hpFill.color = hpLowColor;

        StartCoroutine(ShakeSprite());
    }

    public void PlayDeathAnimation(bool spared)
    {
        StartCoroutine(spared ? SpareAnimation() : DefeatAnimation());
    }

    private IEnumerator ShakeSprite()
    {
        RectTransform rt = enemySprite.rectTransform;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeAmount, shakeAmount);
            rt.anchoredPosition = _originalPos + new Vector3(x, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = _originalPos;
    }

    private IEnumerator DefeatAnimation()
    {
        // Flash white then fade out
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
        // Gently fade to white
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

// ============================================================
// PLAYER STATS UI
// ============================================================
public class PlayerStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpLabel;   // shows "HP XX / XX"
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFill;

    [Header("HP Bar Colors")]
    [SerializeField] private Color hpFullColor = Color.yellow;
    [SerializeField] private Color hpLowColor  = new Color(0.88f, 0.19f, 0.19f);

    [Header("Low HP Threshold")]
    [SerializeField] private float lowHPPercent = 0.3f;

    public void Initialize(int maxHP, int currentHP)
    {
        UpdateHP(currentHP, maxHP);
    }

    public void UpdateHP(int current, int max)
    {
        float pct = (float)current / max;
        hpSlider.value = pct;
        hpLabel.text = $"HP {current} / {max}";
        hpFill.color = pct > lowHPPercent ? hpFullColor : hpLowColor;

        // Pulse animation when critically low
        if (pct <= lowHPPercent)
            StartCoroutine(PulseHP());
    }

    private IEnumerator PulseHP()
    {
        float t = 0f;
        Color startColor = hpFill.color;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float lerp = Mathf.PingPong(t * 2f, 1f);
            hpFill.color = Color.Lerp(startColor, Color.white, lerp * 0.4f);
            yield return null;
        }
        hpFill.color = startColor;
    }
}
