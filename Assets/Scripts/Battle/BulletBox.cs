using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SoulController soul;
    [SerializeField] private RectTransform boxRect;
    [SerializeField] private Transform bulletParent; // Empty child to hold spawned bullets

    [Header("Default Bullet Prefabs")]
    [SerializeField] private GameObject straightBulletPrefab;
    [SerializeField] private GameObject waveBulletPrefab;
    [SerializeField] private GameObject bounceBulletPrefab;

    private List<BulletBase> _activeBullets = new List<BulletBase>();
    private Coroutine _spawnRoutine;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void BeginAttack(AttackPattern pattern)
    {
        soul.Activate();
        _spawnRoutine = StartCoroutine(RunPattern(pattern));
    }

    public void EndAttack()
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        soul.Deactivate();
        ClearAllBullets();
    }

    // -------------------------------------------------------------------------
    // Pattern Runner
    // -------------------------------------------------------------------------

    private IEnumerator RunPattern(AttackPattern pattern)
    {
        if (pattern == null) yield break;

        foreach (var wave in pattern.waves)
        {
            yield return new WaitForSeconds(wave.delayBefore);

            for (int i = 0; i < wave.count; i++)
            {
                SpawnBullet(wave);
                yield return new WaitForSeconds(wave.intervalBetween);
            }
        }
    }

    private void SpawnBullet(BulletWave wave)
    {
        GameObject prefab = wave.overridePrefab != null ? wave.overridePrefab : GetDefaultPrefab(wave.type);
        if (prefab == null) return;

        Vector2 spawnPos = GetSpawnPosition(wave.spawnEdge);
        Vector2 direction = GetBulletDirection(wave.spawnEdge, spawnPos, wave.aimAtSoul);

        GameObject go = Instantiate(prefab, bulletParent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = spawnPos;

        BulletBase bullet = go.GetComponent<BulletBase>();
        bullet?.Initialize(direction, wave.speed, wave.damage, this);

        _activeBullets.Add(bullet);
    }

    // -------------------------------------------------------------------------
    // Spawn Helpers
    // -------------------------------------------------------------------------

    private Vector2 GetSpawnPosition(SpawnEdge edge)
    {
        Vector2 size = boxRect.rect.size;
        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        float rand;

        switch (edge)
        {
            case SpawnEdge.Top:
                rand = Random.Range(-halfW, halfW);
                return new Vector2(rand, halfH + 10f);
            case SpawnEdge.Bottom:
                rand = Random.Range(-halfW, halfW);
                return new Vector2(rand, -halfH - 10f);
            case SpawnEdge.Left:
                rand = Random.Range(-halfH, halfH);
                return new Vector2(-halfW - 10f, rand);
            case SpawnEdge.Right:
                rand = Random.Range(-halfH, halfH);
                return new Vector2(halfW + 10f, rand);
            case SpawnEdge.Random:
            default:
                SpawnEdge[] edges = { SpawnEdge.Top, SpawnEdge.Bottom, SpawnEdge.Left, SpawnEdge.Right };
                return GetSpawnPosition(edges[Random.Range(0, 4)]);
        }
    }

    private Vector2 GetBulletDirection(SpawnEdge edge, Vector2 spawnPos, bool aimAtSoul)
    {
        if (aimAtSoul)
        {
            Vector2 soulPos = soul.GetComponent<RectTransform>().anchoredPosition;
            return (soulPos - spawnPos).normalized;
        }

        switch (edge)
        {
            case SpawnEdge.Top:    return Vector2.down;
            case SpawnEdge.Bottom: return Vector2.up;
            case SpawnEdge.Left:   return Vector2.right;
            case SpawnEdge.Right:  return Vector2.left;
            default:               return Vector2.down;
        }
    }

    private GameObject GetDefaultPrefab(BulletType type)
    {
        switch (type)
        {
            case BulletType.Wave:   return waveBulletPrefab;
            case BulletType.Bounce: return bounceBulletPrefab;
            default:                return straightBulletPrefab;
        }
    }

    // -------------------------------------------------------------------------
    // Bullet Lifecycle
    // -------------------------------------------------------------------------

    public void RemoveBullet(BulletBase bullet)
    {
        _activeBullets.Remove(bullet);
        Destroy(bullet.gameObject);
    }

    public bool IsOutOfBounds(Vector2 pos)
    {
        Vector2 size = boxRect.rect.size;
        float pad = 20f;
        return Mathf.Abs(pos.x) > size.x * 0.5f + pad ||
               Mathf.Abs(pos.y) > size.y * 0.5f + pad;
    }

    private void ClearAllBullets()
    {
        foreach (var b in _activeBullets)
            if (b != null) Destroy(b.gameObject);
        _activeBullets.Clear();
    }
}
