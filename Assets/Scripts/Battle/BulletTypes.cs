using UnityEngine;
using UnityEngine.UI;

// ============================================================
// BASE CLASS
// ============================================================

[RequireComponent(typeof(RectTransform))]
public abstract class BulletBase : MonoBehaviour
{
    protected RectTransform _rt;
    protected Vector2 _direction;
    protected float _speed;
    protected int _damage;
    protected BulletBox _box;
    protected SoulController _soul;

    public virtual void Initialize(Vector2 direction, float speed, int damage, BulletBox box)
    {
        _rt        = GetComponent<RectTransform>();
        _direction = direction.normalized;
        _speed     = speed;
        _damage    = damage;
        _box       = box;
        _soul      = FindFirstObjectByType<SoulController>();
    }

    protected virtual void Update()
    {
        Move();
        CheckSoulCollision();

        if (_box.IsOutOfBounds(_rt.anchoredPosition))
            _box.RemoveBullet(this);
    }

    protected virtual void Move()
    {
        _rt.anchoredPosition += _direction * _speed * Time.deltaTime;
    }

    protected void CheckSoulCollision()
    {
        if (_soul == null || _soul.IsInvincible) return;

        RectTransform soulRT = _soul.GetComponent<RectTransform>();
        Vector2 delta = _rt.anchoredPosition - soulRT.anchoredPosition;
        float combinedRadius = (_rt.rect.width * 0.5f) + 7f; // 7px = soul half-size

        if (delta.magnitude < combinedRadius)
            _soul.OnHit(_damage);
    }
}

// ============================================================
// STRAIGHT BULLET  — moves in a fixed direction
// ============================================================
public class StraightBullet : BulletBase
{
    // No overrides needed — base behaviour is straight movement
}

// ============================================================
// WAVE BULLET  — sine-wave perpendicular to travel direction
// ============================================================
public class WaveBullet : BulletBase
{
    [SerializeField] private float waveFrequency = 3f;
    [SerializeField] private float waveAmplitude = 40f;

    private float _time = 0f;
    private Vector2 _perpendicular;

    public override void Initialize(Vector2 direction, float speed, int damage, BulletBox box)
    {
        base.Initialize(direction, speed, damage, box);
        // Perpendicular vector for wave offset
        _perpendicular = new Vector2(-_direction.y, _direction.x);
    }

    protected override void Move()
    {
        _time += Time.deltaTime;
        float wave = Mathf.Sin(_time * waveFrequency) * waveAmplitude;
        Vector2 waveOffset = _perpendicular * wave * Time.deltaTime;
        _rt.anchoredPosition += _direction * _speed * Time.deltaTime + waveOffset;
    }
}

// ============================================================
// BOUNCE BULLET  — bounces off the bullet box walls
// ============================================================
public class BounceBullet : BulletBase
{
    private Vector2 _velocity;
    private RectTransform _boxRect;

    public override void Initialize(Vector2 direction, float speed, int damage, BulletBox box)
    {
        base.Initialize(direction, speed, damage, box);
        _velocity = direction.normalized * speed;
        _boxRect = box.GetComponent<RectTransform>();
    }

    protected override void Move()
    {
        _rt.anchoredPosition += _velocity * Time.deltaTime;

        Vector2 size = _boxRect.rect.size;
        float halfW = size.x * 0.5f - _rt.rect.width * 0.5f;
        float halfH = size.y * 0.5f - _rt.rect.height * 0.5f;

        Vector2 pos = _rt.anchoredPosition;

        if (pos.x < -halfW || pos.x > halfW)
        {
            _velocity.x *= -1f;
            pos.x = Mathf.Clamp(pos.x, -halfW, halfW);
        }
        if (pos.y < -halfH || pos.y > halfH)
        {
            _velocity.y *= -1f;
            pos.y = Mathf.Clamp(pos.y, -halfH, halfH);
        }

        _rt.anchoredPosition = pos;
    }

    // Bounce bullets never go "out of bounds" — they bounce forever until turn ends
    protected override void Update()
    {
        Move();
        CheckSoulCollision();
    }
}
