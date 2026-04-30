using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Battle/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Menehune";
    public Sprite enemySprite;
    public bool isMenehuneLeader = false;

    [Header("Stats")]
    public int maxHP = 80;
    public int level = 3;

    [Header("Rewards")]
    public int killEXP = 30;
    public int gold = 10;

    [Header("Damage Ranges")]
    public int minEnemyDamage = 2;
    public int maxEnemyDamage = 6;
    public int minPlayerDamage = 10;
    public int maxPlayerDamage = 20;

    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string introDialogue = "* A mischievous Menehune appears!\n* It eyes your bento hungrily...";

    [TextArea(2, 4)]
    public string[] attackDialogues = {
        "* The Menehune hurls a rock!",
        "* It dashes at you!",
        "* The Menehune throws a coconut!"
    };

    [TextArea(2, 4)]
    public string[] idleDialogues = {
        "* The Menehune watches you carefully.",
        "* It sniffs the air hungrily.",
        "* The Menehune taps its foot impatiently."
    };

    [TextArea(2, 4)]
    public string checkDialogue = "MENEHUNE  ATK 5  DEF 2\n* A small but fierce forest spirit.\n* Loves bento boxes.";

    [Header("Act Options")]
    public ActOption[] actOptions = {
        new ActOption { actName = "Check",      mercyGain = 0,  dialogue = "" },
        new ActOption { actName = "Taunt",       mercyGain = 0,  dialogue = "* You call it tiny.\n* The Menehune looks offended." },
        new ActOption { actName = "Compliment",  mercyGain = 30, dialogue = "* You say it looks mighty for its size.\n* The Menehune seems pleased..." }
    };
}

[System.Serializable]
public class ActOption
{
    public string actName;
    public int mercyGain;
    [TextArea(2, 3)]
    public string dialogue;
}
