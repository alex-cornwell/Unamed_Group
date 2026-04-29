using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattlePhase
{
    INTRO, PLAYER_MENU, PLAYER_ACTION, ENEMY_TURN, VICTORY, GAME_OVER
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle State")]
    public BattlePhase CurrentPhase { get; private set; }
    public int MercyPercent { get; private set; } = 0;

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;
    private int currentEnemyHP;

    [Header("Player Data")]
    [SerializeField] private int playerMaxHP = 20;
    private int currentPlayerHP;

    [Header("UI References")]
    [SerializeField] private DialogueTyper dialogueTyper;
    [SerializeField] private ActionMenu actionMenu;
    [SerializeField] private BulletBox bulletBox;
    [SerializeField] private EnemyUI enemyUI;
    [SerializeField] private PlayerStatsUI playerStatsUI;
    [SerializeField] private BattleInventoryUI battleInventoryUI;

    [Header("Battle Canvas")]
    [SerializeField] private GameObject battleCanvas;

    // Events
    public System.Action<BattlePhase> OnPhaseChanged;
    public System.Action<int, int> OnPlayerHPChanged;
    public System.Action<int, int> OnEnemyHPChanged;
    public System.Action<int> OnMercyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Load enemy from PlayerPrefs
        string enemyName = PlayerPrefs.GetString("CurrentEnemy", "MenehuneData");
        enemyData = Resources.Load<EnemyData>($"EnemyData/{enemyName}");

        if (enemyData == null)
        {
            Debug.LogError($"EnemyData '{enemyName}' not found in Resources/EnemyData/");
            return;
        }

        currentEnemyHP = enemyData.maxHP;
        currentPlayerHP = playerMaxHP;
        StartCoroutine(StartBattle());
    }

    // -------------------------------------------------------------------------
    // Phase Control
    // -------------------------------------------------------------------------

    private IEnumerator StartBattle()
    {
        SetPhase(BattlePhase.INTRO);
        enemyUI.Initialize(enemyData, currentEnemyHP);
        playerStatsUI.Initialize(playerMaxHP, currentPlayerHP);
        battleInventoryUI?.LoadInventory();

        yield return new WaitForSeconds(1f);
        yield return dialogueTyper.TypeDialogue(enemyData.introDialogue);

        SetPhase(BattlePhase.PLAYER_MENU);
        actionMenu.ShowMainMenu();
    }

    public void SetPhase(BattlePhase phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
    }

    // -------------------------------------------------------------------------
    // Player Actions
    // -------------------------------------------------------------------------

    public void PlayerFight()
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(FightRoutine());
    }

    private IEnumerator FightRoutine()
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();

        int damage = Random.Range(enemyData.minPlayerDamage, enemyData.maxPlayerDamage + 1);
        DamageEnemy(damage);

        string msg = $"* You strike with full force!\n* {enemyData.enemyName} takes {damage} damage!";
        yield return dialogueTyper.TypeDialogue(msg);

        if (currentEnemyHP <= 0) { StartCoroutine(VictoryRoutine(false)); yield break; }
        StartCoroutine(EnemyTurnRoutine());
    }

    public void PlayerAct(string actName, int mercyGain, string dialogue)
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(ActRoutine(actName, mercyGain, dialogue));
    }

    private IEnumerator ActRoutine(string actName, int mercyGain, string dialogue)
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();

        AddMercy(mercyGain);
        yield return dialogueTyper.TypeDialogue(dialogue);
        StartCoroutine(EnemyTurnRoutine());
    }

    // -------------------------------------------------------------------------
    // BENTO TRADE — ends battle, drops drive belt if leader
    // -------------------------------------------------------------------------

    public void PlayerTradeBento()
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;

        // Check player has bento in inventory
        if (!battleInventoryUI.HasItem("Bento"))
        {
            StartCoroutine(NoBentoRoutine());
            return;
        }

        StartCoroutine(TradeRoutine());
    }

    private IEnumerator NoBentoRoutine()
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();
        yield return dialogueTyper.TypeDialogue("* You reach for a bento...\n* But you don't have any!");
        SetPhase(BattlePhase.PLAYER_MENU);
        actionMenu.ShowMainMenu();
    }

    private IEnumerator TradeRoutine()
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();

        // Consume bento from inventory
        battleInventoryUI.ConsumeItem("Bento");

        if (enemyData.isMenehuneLeader)
        {
            yield return dialogueTyper.TypeDialogue(
                $"* You offer the bento to the Menehune leader.\n* His eyes light up!\n* He accepts the offering...\n* The Menehune drop a Drive Belt and leave!");
            StartCoroutine(VictoryRoutine(true, dropDriveBelt: true));
        }
        else
        {
            yield return dialogueTyper.TypeDialogue(
                $"* You offer the bento to the Menehune.\n* It grabs it and scurries away!");
            StartCoroutine(VictoryRoutine(true, dropDriveBelt: false));
        }
    }

    // -------------------------------------------------------------------------
    // ITEM USE — bento heals player
    // -------------------------------------------------------------------------

    public void UseItem(string itemName)
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(UseItemRoutine(itemName));
    }

    private IEnumerator UseItemRoutine(string itemName)
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();

        if (itemName == "Bento")
        {
            if (!battleInventoryUI.HasItem("Bento"))
            {
                yield return dialogueTyper.TypeDialogue("* You reach for a bento...\n* But you don't have any!");
            }
            else
            {
                battleInventoryUI.ConsumeItem("Bento");
                int healAmount = 8;
                HealPlayer(healAmount);
                yield return dialogueTyper.TypeDialogue($"* You eat the bento box.\n* Restored {healAmount} HP!");
            }
        }
        else
        {
            yield return dialogueTyper.TypeDialogue($"* You used {itemName}.\n* Nothing happened...");
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    public void PlayerMercy()
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(MercyRoutine());
    }

    private IEnumerator MercyRoutine()
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();

        if (MercyPercent >= 100) { StartCoroutine(VictoryRoutine(true)); yield break; }

        AddMercy(20);
        string msg = MercyPercent >= 100
            ? $"* {enemyData.enemyName} wants to stop fighting..."
            : $"* You show mercy. ({MercyPercent}% mercy)";

        yield return dialogueTyper.TypeDialogue(msg);
        if (MercyPercent >= 100) { StartCoroutine(VictoryRoutine(true)); yield break; }
        StartCoroutine(EnemyTurnRoutine());
    }

    // -------------------------------------------------------------------------
    // Enemy Turn
    // -------------------------------------------------------------------------

    private IEnumerator EnemyTurnRoutine()
    {
        SetPhase(BattlePhase.ENEMY_TURN);

        string attackMsg = enemyData.attackDialogues[Random.Range(0, enemyData.attackDialogues.Length)];
        dialogueTyper.TypeDialogueNoWait(attackMsg);

        bulletBox.gameObject.SetActive(true);
        bulletBox.BeginAttack(enemyData.currentAttackPattern);

        yield return new WaitForSeconds(enemyData.turnDuration);

        bulletBox.EndAttack();
        bulletBox.gameObject.SetActive(false);

        ReturnToPlayerMenu();
    }

    public void TakeDamage(int amount)
    {
        currentPlayerHP = Mathf.Max(0, currentPlayerHP - amount);
        OnPlayerHPChanged?.Invoke(currentPlayerHP, playerMaxHP);
        playerStatsUI.UpdateHP(currentPlayerHP, playerMaxHP);

        if (currentPlayerHP <= 0)
        {
            bulletBox.EndAttack();
            StartCoroutine(GameOverRoutine());
        }
    }

    private void ReturnToPlayerMenu()
    {
        SetPhase(BattlePhase.PLAYER_MENU);
        string idleMsg = enemyData.idleDialogues[Random.Range(0, enemyData.idleDialogues.Length)];
        dialogueTyper.TypeDialogueNoWait(idleMsg);
        actionMenu.ShowMainMenu();
    }

    // -------------------------------------------------------------------------
    // Victory / Game Over
    // -------------------------------------------------------------------------

    private IEnumerator VictoryRoutine(bool spared, bool dropDriveBelt = false)
    {
        SetPhase(BattlePhase.VICTORY);
        actionMenu.HideAll();
        bulletBox.gameObject.SetActive(false);

        if (!spared)
        {
            string msg = $"* {enemyData.enemyName} was defeated!\n* {enemyData.killEXP} EXP  {enemyData.gold} GOLD";
            enemyUI.PlayDeathAnimation(false);
            yield return dialogueTyper.TypeDialogue(msg);
        }

        // Save drop belt flag for world scene
        if (dropDriveBelt)
            PlayerPrefs.SetInt("DropDriveBelt", 1);

        yield return new WaitForSeconds(1.5f);
        string returnScene = PlayerPrefs.GetString("ReturnScene", "SampleScene");
        SceneManager.LoadScene(returnScene);
    }

    private IEnumerator GameOverRoutine()
    {
        SetPhase(BattlePhase.GAME_OVER);
        yield return dialogueTyper.TypeDialogue("* ...\n* YOU DIED\n\n* But it refused.");
        yield return new WaitForSeconds(2f);

        currentPlayerHP = 1;
        OnPlayerHPChanged?.Invoke(currentPlayerHP, playerMaxHP);
        playerStatsUI.UpdateHP(currentPlayerHP, playerMaxHP);
        yield return dialogueTyper.TypeDialogue("* DETERMINATION.\n* You stand back up.");
        yield return new WaitForSeconds(1f);

        string returnScene = PlayerPrefs.GetString("ReturnScene", "SampleScene");
        SceneManager.LoadScene(returnScene);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void DamageEnemy(int amount)
    {
        currentEnemyHP = Mathf.Max(0, currentEnemyHP - amount);
        OnEnemyHPChanged?.Invoke(currentEnemyHP, enemyData.maxHP);
        enemyUI.UpdateHP(currentEnemyHP, enemyData.maxHP);
    }

    private void HealPlayer(int amount)
    {
        currentPlayerHP = Mathf.Min(playerMaxHP, currentPlayerHP + amount);
        OnPlayerHPChanged?.Invoke(currentPlayerHP, playerMaxHP);
        playerStatsUI.UpdateHP(currentPlayerHP, playerMaxHP);
    }

    private void AddMercy(int amount)
    {
        MercyPercent = Mathf.Min(100, MercyPercent + amount);
        OnMercyChanged?.Invoke(MercyPercent);
    }
}
