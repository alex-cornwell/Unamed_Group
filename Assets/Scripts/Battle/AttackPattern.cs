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
    [Tooltip("Seconds to wait before this wave starts")]
    public float delayBefore = 0f;
    [Tooltip("Number of bullets in this wave")]
    public int count = 1;
    [Tooltip("Seconds between each bullet in the wave")]
    public float intervalBetween = 0.2f;

    [Header("Bullet Properties")]
    public BulletType type = BulletType.Straight;
    public SpawnEdge spawnEdge = SpawnEdge.Random;
    public float speed = 200f;
    public int damage = 4;
    [Tooltip("If true, bullet aims directly at the soul's current position")]
    public bool aimAtSoul = false;

    [Header("Override")]
    [Tooltip("Leave null to use the BulletBox default prefab for this BulletType")]
    public GameObject overridePrefab;
}
