using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpLabel;
    [SerializeField] private Slider          hpSlider;
    [SerializeField] private Image           hpFill;

    [Header("HP Bar Colors")]
    [SerializeField] private Color hpFullColor = Color.yellow;
    [SerializeField] private Color hpLowColor  = new Color(0.88f, 0.19f, 0.19f);

    [Header("Low HP Threshold")]
    [SerializeField] private float lowHPPercent = 0.3f;

    [Header("HP Bar Drain")]
    [SerializeField] private float hpDrainSpeed = 1.5f; // slider units per second

    private float _targetHP  = 1f;
    private float _displayHP = 1f;

    public void Initialize(int maxHP, int currentHP)
    {
        _targetHP  = (float)currentHP / maxHP;
        _displayHP = _targetHP;
        hpSlider.value = _displayHP;
        UpdateLabel(currentHP, maxHP);
        UpdateColor(_targetHP);
    }

    private void Update()
    {
        if (!Mathf.Approximately(_displayHP, _targetHP))
        {
            _displayHP = Mathf.MoveTowards(_displayHP, _targetHP, hpDrainSpeed * Time.deltaTime);
            hpSlider.value = _displayHP;
        }
    }

    public void UpdateHP(int current, int max, int damageTaken = 0, int healAmount = 0)
    {
        float pct  = (float)current / max;
        _targetHP  = pct;

        UpdateLabel(current, max);
        UpdateColor(pct);

        if (damageTaken > 0)
        {
            ScreenEffects.Instance?.PlayerHit();

            if (pct <= lowHPPercent)
                StartCoroutine(PulseHP());
        }
    }

    private void UpdateLabel(int current, int max)
    {
        hpLabel.text = $"HP {current} / {max}";
    }

    private void UpdateColor(float pct)
    {
        hpFill.color = pct > lowHPPercent ? hpFullColor : hpLowColor;
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
