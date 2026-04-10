using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BattlePhase
{
    INTRO,
    PLAYER_MENU,
    PLAYER_ACTION,
    ENEMY_TURN,
    VICTORY,
    GAME_OVER
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

    [Header("Battle Settings")]
    [SerializeField] private float introDelay = 1.5f;

    // Events
    public System.Action<BattlePhase> OnPhaseChanged;
    public System.Action<int, int> OnPlayerHPChanged;   // current, max
    public System.Action<int, int> OnEnemyHPChanged;    // current, max
    public System.Action<int> OnMercyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
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

        yield return new WaitForSeconds(introDelay);
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
    // Player Actions (called by ActionMenu buttons)
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

        string msg = $"* You attack with full force!\n* {enemyData.enemyName} takes {damage} damage!";
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

    public void PlayerItem(int healAmount, string dialogue)
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(ItemRoutine(healAmount, dialogue));
    }

    private IEnumerator ItemRoutine(int healAmount, string dialogue)
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();

        HealPlayer(healAmount);
        yield return dialogueTyper.TypeDialogue(dialogue);
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

        if (MercyPercent >= 100)
        {
            StartCoroutine(VictoryRoutine(true));
            yield break;
        }

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

        int dmg = Random.Range(enemyData.minEnemyDamage, enemyData.maxEnemyDamage + 1);
        // Reduce by bullets dodged — BulletBox reports hits separately via TakeDamage()

        ReturnToPlayerMenu();
    }

    public void TakeDamage(int amount)
    {
        // Called by BulletBox when a bullet hits the soul
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

    private IEnumerator VictoryRoutine(bool spared)
    {
        SetPhase(BattlePhase.VICTORY);
        actionMenu.HideAll();
        bulletBox.gameObject.SetActive(false);

        string msg = spared
            ? $"* ...\n* {enemyData.enemyName} lowers its guard.\n* You spared {enemyData.enemyName}.\n* {enemyData.spareEXP} EXP gained."
            : $"* {enemyData.enemyName} was defeated!\n* {enemyData.killEXP} EXP  {enemyData.gold} GOLD";

        enemyUI.PlayDeathAnimation(spared);
        yield return dialogueTyper.TypeDialogue(msg);
    }

    private IEnumerator GameOverRoutine()
    {
        SetPhase(BattlePhase.GAME_OVER);
        yield return dialogueTyper.TypeDialogue("* ...\n* YOU DIED\n\n* But it refused.");
        yield return new WaitForSeconds(2f);

        // Determination — revive with 1 HP
        currentPlayerHP = 1;
        OnPlayerHPChanged?.Invoke(currentPlayerHP, playerMaxHP);
        playerStatsUI.UpdateHP(currentPlayerHP, playerMaxHP);
        yield return dialogueTyper.TypeDialogue("* DETERMINATION.\n* You stand back up.");
        ReturnToPlayerMenu();
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
