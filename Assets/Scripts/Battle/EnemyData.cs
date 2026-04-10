using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Glitch Beast";
    public Sprite enemySprite;

    [Header("Stats")]
    public int maxHP = 320;
    public int attack = 8;
    public int defense = 4;
    public int level = 7;

    [Header("Rewards")]
    public int killEXP = 50;
    public int spareEXP = 30;
    public int gold = 30;

    [Header("Damage Ranges")]
    public int minEnemyDamage = 4;
    public int maxEnemyDamage = 10;
    public int minPlayerDamage = 25;
    public int maxPlayerDamage = 45;

    [Header("Timing")]
    [Tooltip("How long the enemy attack phase lasts in seconds")]
    public float turnDuration = 5f;

    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string introDialogue = "* The GLITCH BEAST blocks your path!\n* Its pixels flicker ominously.";

    [TextArea(2, 4)]
    public string[] attackDialogues = {
        "* GLITCH BEAST charges its pixels!",
        "* It fires a burst of static!",
        "* Watch out for the data shards!"
    };

    [TextArea(2, 4)]
    public string[] idleDialogues = {
        "* GLITCH BEAST watches you cautiously.",
        "* Its form flickers slightly.",
        "* GLITCH BEAST tilts its head."
    };

    [TextArea(2, 4)]
    public string checkDialogue = "GLITCH BEAST  ATK 8  DEF 4\n* A creature born from corrupted data.\n* It just wants to be understood.";

    [Header("Act Options")]
    public ActOption[] actOptions = {
        new ActOption { actName = "Check",      mercyGain = 0,  dialogue = "" }, // set at runtime from checkDialogue
        new ActOption { actName = "Taunt",       mercyGain = 0,  dialogue = "* You call it a walking bug report.\n* GLITCH BEAST looks annoyed." },
        new ActOption { actName = "Compliment",  mercyGain = 40, dialogue = "* You say its glitches look kind of cool.\n* GLITCH BEAST seems... flustered?" }
    };

    [Header("Attack Pattern")]
    public AttackPattern currentAttackPattern;
}

[System.Serializable]
public class ActOption
{
    public string actName;
    [Tooltip("How much this act fills the mercy meter (0-100)")]
    public int mercyGain;
    [TextArea(2, 3)]
    public string dialogue;
}
