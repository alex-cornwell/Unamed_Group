using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattlePhase
{
    INTRO, PLAYER_MENU, PLAYER_ACTION, ENEMY_TURN, VICTORY, GAME_OVER
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public BattlePhase CurrentPhase { get; private set; }

    private EnemyData enemyData;
    private int currentEnemyHP;

    [SerializeField] private int playerMaxHP = 20;
    private int currentPlayerHP;

    [Header("Tool Damage Boost")]
    [SerializeField] private int hammerDamageBonus = 8;
    [SerializeField] private int drillDamageBonus  = 10;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel; // assign in Inspector

    [SerializeField] private DialogueTyper    dialogueTyper;
    [SerializeField] private ActionMenu       actionMenu;
    [SerializeField] private EnemyUI          enemyUI;
    [SerializeField] private PlayerStatsUI    playerStatsUI;
    [SerializeField] private BattleInventoryUI battleInventoryUI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            foreach (var al in listeners)
            {
                if (al.gameObject.scene.name == "BattleScene")
                    al.enabled = false;
            }
        }
    }

    private void Start()
    {
        string enemyName = PlayerPrefs.GetString("CurrentEnemy", "MenehuneData");
        enemyData = Resources.Load<EnemyData>($"EnemyData/{enemyName}");
        if (enemyData == null) { Debug.LogError($"EnemyData '{enemyName}' not found"); return; }

        enemyData.isMenehuneLeader = PlayerPrefs.GetInt("IsMenehuneLeader", 0) == 1;

        currentEnemyHP  = enemyData.maxHP;

        // Load saved HP so it persists between battles
        currentPlayerHP = PlayerPrefs.GetInt("PlayerHP", playerMaxHP);
        currentPlayerHP = Mathf.Clamp(currentPlayerHP, 1, playerMaxHP);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle()
    {
        SetPhase(BattlePhase.INTRO);
        enemyUI.Initialize(enemyData, currentEnemyHP);
        playerStatsUI.Initialize(playerMaxHP, currentPlayerHP);
        battleInventoryUI?.LoadInventory();
        actionMenu.Initialize(enemyData);

        yield return new WaitForSeconds(0.5f);
        yield return dialogueTyper.TypeDialogue(enemyData.introDialogue);
        SetPhase(BattlePhase.PLAYER_MENU);
        actionMenu.ShowMainMenu();
    }

    public void SetPhase(BattlePhase phase) => CurrentPhase = phase;

    // ── FIGHT ────────────────────────────────────────────────────────────────

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

        bool hasHammer = battleInventoryUI != null && battleInventoryUI.HasItem("Hammer");
        bool hasDrill  = battleInventoryUI != null && battleInventoryUI.HasItem("Drill");

        string bonusText = "";
        if (hasHammer && hasDrill)
        {
            damage   += Mathf.Max(hammerDamageBonus, drillDamageBonus);
            bonusText = "\n* Your tools give you strength!";
        }
        else if (hasHammer)
        {
            damage   += hammerDamageBonus;
            bonusText = "\n* The hammer lands hard!";
        }
        else if (hasDrill)
        {
            damage   += drillDamageBonus;
            bonusText = "\n* The drill whirs fiercely!";
        }

        DamageEnemy(damage);
        yield return dialogueTyper.TypeDialogue(
            $"* You attack!\n* {enemyData.enemyName} takes {damage} damage!{bonusText}");

        if (currentEnemyHP <= 0) { StartCoroutine(VictoryRoutine(false)); yield break; }
        StartCoroutine(EnemyTurnRoutine());
    }

    // ── ACT ──────────────────────────────────────────────────────────────────

    public void PlayerAct(string actName, int mercyGain, string dialogue)
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(ActRoutine(dialogue));
    }

    private IEnumerator ActRoutine(string dialogue)
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();
        yield return dialogueTyper.TypeDialogue(dialogue);
        StartCoroutine(EnemyTurnRoutine());
    }

    // ── TRADE BENTO ──────────────────────────────────────────────────────────

    public void PlayerTradeBento()
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        if (battleInventoryUI == null || !battleInventoryUI.HasItem("Bento"))
        {
            StartCoroutine(SimpleDialogue(
                "* You reach for a bento...\n* But you don't have any!", returnToMenu: true));
            return;
        }
        StartCoroutine(TradeRoutine());
    }

    private IEnumerator TradeRoutine()
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();
        battleInventoryUI.ConsumeItem("Bento");
        if (enemyData.isMenehuneLeader)
        {
            yield return dialogueTyper.TypeDialogue(
                "* You offer the bento to the Menehune leader.\n* His eyes light up!\n* He accepts... The Menehune leave!");
            StartCoroutine(VictoryRoutine(true, dropDriveBelt: true));
        }
        else
        {
            yield return dialogueTyper.TypeDialogue(
                "* You offer the bento to the Menehune.\n* It grabs it and scurries away!");
            StartCoroutine(VictoryRoutine(true));
        }
    }

    // ── USE ITEM ─────────────────────────────────────────────────────────────

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
            if (battleInventoryUI == null || !battleInventoryUI.HasItem("Bento"))
                yield return dialogueTyper.TypeDialogue(
                    "* You reach for a bento...\n* But you don't have any!");
            else
            {
                battleInventoryUI.ConsumeItem("Bento");
                int heal = 8;
                HealPlayer(heal);
                yield return dialogueTyper.TypeDialogue(
                    $"* You eat the bento box.\n* Restored {heal} HP!");
            }
        }
        else if (itemName == "Hammer")
        {
            if (enemyData.isMenehuneLeader)
            {
                battleInventoryUI.ConsumeItem("Hammer");
                yield return dialogueTyper.TypeDialogue(
                    "* You offer your hammer to the Menehune leader.\n* He examines it carefully...\n* A deal is struck! The Menehune retreat!");
                StartCoroutine(VictoryRoutine(true, dropDriveBelt: true));
                yield break;
            }
            else
                yield return dialogueTyper.TypeDialogue(
                    "* You brandish the hammer!\n* It deals extra damage when you FIGHT!");
        }
        else if (itemName == "Drill")
        {
            if (enemyData.isMenehuneLeader)
            {
                battleInventoryUI.ConsumeItem("Drill");
                yield return dialogueTyper.TypeDialogue(
                    "* You offer your drill to the Menehune leader.\n* His eyes go wide!\n* He accepts — the Menehune scatter!");
                StartCoroutine(VictoryRoutine(true, dropDriveBelt: true));
                yield break;
            }
            else
                yield return dialogueTyper.TypeDialogue(
                    "* The drill whirs loudly!\n* It deals extra damage when you FIGHT!");
        }
        else if (itemName == "Drive Belt")
            yield return dialogueTyper.TypeDialogue(
                "* You hold up the drive belt.\n* It doesn't do much here...");
        else
            yield return dialogueTyper.TypeDialogue(
                $"* You used {itemName}.\n* Nothing happened...");

        StartCoroutine(EnemyTurnRoutine());
    }

    // ── RUN ──────────────────────────────────────────────────────────────────

    public void PlayerRun()
    {
        if (CurrentPhase != BattlePhase.PLAYER_MENU) return;
        StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();
        yield return dialogueTyper.TypeDialogue("* You run away!");
        yield return new WaitForSeconds(0.5f);

        // Save current HP before leaving
        PlayerPrefs.SetInt("PlayerHP", currentPlayerHP);
        PlayerPrefs.SetInt("BattleWon", 0);
        PlayerPrefs.SetInt("PlayerRan", 1);
        PlayerPrefs.Save();

        StartCoroutine(ReturnToWorld(battleWon: false));
    }

    // ── ENEMY TURN ───────────────────────────────────────────────────────────

    private IEnumerator EnemyTurnRoutine()
    {
        SetPhase(BattlePhase.ENEMY_TURN);
        int damage = Random.Range(enemyData.minEnemyDamage, enemyData.maxEnemyDamage + 1);
        DamagePlayer(damage);
        string msg = enemyData.attackDialogues[Random.Range(0, enemyData.attackDialogues.Length)];
        yield return dialogueTyper.TypeDialogue($"{msg}\n* You took {damage} damage!");
        if (currentPlayerHP <= 0) { StartCoroutine(GameOverRoutine()); yield break; }
        dialogueTyper.TypeDialogueNoWait(
            enemyData.idleDialogues[Random.Range(0, enemyData.idleDialogues.Length)]);
        SetPhase(BattlePhase.PLAYER_MENU);
        actionMenu.ShowMainMenu();
    }

    // ── VICTORY ──────────────────────────────────────────────────────────────

    private IEnumerator VictoryRoutine(bool spared, bool dropDriveBelt = false)
    {
        SetPhase(BattlePhase.VICTORY);
        actionMenu.HideAll();
        enemyUI.PlayDeathAnimation(spared);
        if (!spared)
            yield return dialogueTyper.TypeDialogue(
                $"* {enemyData.enemyName} was defeated!\n* {enemyData.killEXP} EXP  {enemyData.gold} GOLD");

        // Save HP after winning
        PlayerPrefs.SetInt("PlayerHP", currentPlayerHP);
        PlayerPrefs.SetInt("BattleWon", 1);
        PlayerPrefs.SetInt("PlayerRan", 0);
        if (dropDriveBelt) PlayerPrefs.SetInt("DropDriveBelt", 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(1.5f);
        StartCoroutine(ReturnToWorld(battleWon: true));
    }

    // ── GAME OVER ────────────────────────────────────────────────────────────

    private IEnumerator GameOverRoutine()
    {
        SetPhase(BattlePhase.GAME_OVER);
        actionMenu.HideAll();

        yield return dialogueTyper.TypeDialogue("* ...\n* YOU WERE DEFEATED.");
        yield return new WaitForSeconds(1f);

        // Reset HP to full on game over
        PlayerPrefs.SetInt("PlayerHP", playerMaxHP);
        PlayerPrefs.Save();

        // Show game over panel which has Return to Title button
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            // Fallback if no panel — go straight to title
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("TitleScreen");
        }
    }

    // Called by the Game Over panel's Return to Title button
    public void OnGameOverReturnToTitle()
    {
        PlayerPrefs.DeleteKey("BattleInventory");
        PlayerPrefs.DeleteKey("BattleInventoryItems");
        PlayerPrefs.DeleteKey("BattleHotbarItems");
        PlayerPrefs.DeleteKey("ItemsConsumed");
        PlayerPrefs.DeleteKey("ReturningFromBattle");
        PlayerPrefs.Save();
        SceneManager.LoadScene("TitleScreen");
    }

    // ── HELPERS ──────────────────────────────────────────────────────────────

    private IEnumerator SimpleDialogue(string msg, bool returnToMenu = false)
    {
        SetPhase(BattlePhase.PLAYER_ACTION);
        actionMenu.HideAll();
        yield return dialogueTyper.TypeDialogue(msg);
        if (returnToMenu) { SetPhase(BattlePhase.PLAYER_MENU); actionMenu.ShowMainMenu(); }
    }

    private void DamageEnemy(int amount)
    {
        int actual = Mathf.Min(amount, currentEnemyHP);
        currentEnemyHP = Mathf.Max(0, currentEnemyHP - amount);
        enemyUI.UpdateHP(currentEnemyHP, enemyData.maxHP, damageTaken: actual);
    }

    private void DamagePlayer(int amount)
    {
        int actual = Mathf.Min(amount, currentPlayerHP);
        currentPlayerHP = Mathf.Max(0, currentPlayerHP - amount);
        playerStatsUI.UpdateHP(currentPlayerHP, playerMaxHP, damageTaken: actual);
    }

    private void HealPlayer(int amount)
    {
        currentPlayerHP = Mathf.Min(playerMaxHP, currentPlayerHP + amount);
        playerStatsUI.UpdateHP(currentPlayerHP, playerMaxHP, healAmount: amount);
    }

    private IEnumerator ReturnToWorld(bool battleWon)
    {
        string returnScene = PlayerPrefs.GetString("ReturnScene", "Minigame1");

        bool itemsWereConsumed = battleInventoryUI != null && battleInventoryUI.WereItemsConsumed();
        PlayerPrefs.SetInt("ItemsConsumed", itemsWereConsumed ? 1 : 0);
        PlayerPrefs.Save();

        if (SceneManager.GetSceneByName(returnScene).isLoaded)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(returnScene));

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerMovement pm = player.GetComponent<PlayerMovement>();
                if (pm != null) pm.enabled = true;
            }

            PauseController.SetPause(false);

            WorldBattleReturn worldReturn = FindFirstObjectByType<WorldBattleReturn>();
            if (worldReturn != null)
                yield return worldReturn.StartCoroutine(worldReturn.HandleReturn(battleWon));

            yield return SceneManager.UnloadSceneAsync("BattleScene");
        }
        else
        {
            SceneManager.LoadScene(returnScene);
        }
    }
}
