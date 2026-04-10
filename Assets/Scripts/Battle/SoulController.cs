using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SoulController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 220f;

    [Header("Bounds")]
    [SerializeField] private RectTransform bulletBoxRect; // The play area
    [SerializeField] private float soulHalfSize = 7f;     // Half width/height of the soul hitbox

    [Header("Visual")]
    [SerializeField] private Image soulImage;
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color invincibleColor = new Color(1f, 0.4f, 0.4f, 0.5f);

    [Header("Invincibility Frames")]
    [SerializeField] private float iFrameDuration = 0.6f;

    private RectTransform _rt;
    private bool _active = false;
    private bool _invincible = false;
    private float _iFrameTimer = 0f;

    private Vector2 _minBounds;
    private Vector2 _maxBounds;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    public void Activate()
    {
        _active = true;
        _invincible = false;
        _iFrameTimer = 0f;
        RecalculateBounds();
        CenterInBox();
        soulImage.color = normalColor;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        _active = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_active) return;

        HandleMovement();
        HandleIFrames();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 delta = new Vector2(h, v).normalized * moveSpeed * Time.deltaTime;
        Vector2 newPos = _rt.anchoredPosition + delta;

        newPos.x = Mathf.Clamp(newPos.x, _minBounds.x, _maxBounds.x);
        newPos.y = Mathf.Clamp(newPos.y, _minBounds.y, _maxBounds.y);

        _rt.anchoredPosition = newPos;
    }

    private void HandleIFrames()
    {
        if (!_invincible) return;

        _iFrameTimer -= Time.deltaTime;
        // Flicker effect during iframes
        soulImage.enabled = Mathf.Sin(_iFrameTimer * 30f) > 0f;

        if (_iFrameTimer <= 0f)
        {
            _invincible = false;
            soulImage.enabled = true;
            soulImage.color = normalColor;
        }
    }

    public void TriggerIFrames()
    {
        _invincible = true;
        _iFrameTimer = iFrameDuration;
        soulImage.color = invincibleColor;
    }

    public bool IsInvincible => _invincible;

    // Called by bullet collision
    public void OnHit(int damage)
    {
        if (_invincible) return;
        BattleManager.Instance.TakeDamage(damage);
        TriggerIFrames();
    }

    private void RecalculateBounds()
    {
        Vector2 size = bulletBoxRect.rect.size;
        Vector2 pivot = bulletBoxRect.pivot;

        float left   = -size.x * pivot.x       + soulHalfSize;
        float right  =  size.x * (1f - pivot.x) - soulHalfSize;
        float bottom = -size.y * pivot.y        + soulHalfSize;
        float top    =  size.y * (1f - pivot.y) - soulHalfSize;

        _minBounds = new Vector2(left, bottom);
        _maxBounds = new Vector2(right, top);
    }

    private void CenterInBox()
    {
        _rt.anchoredPosition = Vector2.zero;
    }
}
