using UnityEngine;

public enum BulletType  { Straight, Wave, Bounce }
public enum SpawnEdge   { Top, Bottom, Left, Right, Random }

[CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Battle/Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    public BulletWave[] waves;
}

[System.Serializable]
public class BulletWave
{
    [Header("Timing")]
    public float delayBefore      = 0f;
    public int   count            = 1;
    public float intervalBetween  = 0.2f;

    [Header("Bullet Properties")]
    public BulletType type      = BulletType.Straight;
    public SpawnEdge  spawnEdge = SpawnEdge.Random;
    public float      speed     = 200f;
    public int        damage    = 4;
    public bool       aimAtSoul = false;

    [Header("Override")]
    public GameObject overridePrefab;
}
