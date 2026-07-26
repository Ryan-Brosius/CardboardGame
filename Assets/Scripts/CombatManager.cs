using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandController hand;
    [SerializeField] private DeckManager deck;
    [SerializeField] private Transform enemySpawnParent;
    [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Anim_PlayerAttack playerAttackAnim;

    [Header("Combat Values")]
    [SerializeField] private int startingSword = 100;   // basically just a test for health move later
    [SerializeField] private int drawPerTurn = 3;
    [SerializeField] private float enemyAttackDelay = 0.6f;

    [Header("Events")]
    public UnityEvent<int> onSwordChanged;
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<int> onPlayerDamaged;
    public UnityEvent<bool> onBlockChanged;
    public UnityEvent<bool> onDefendChanged;
    public UnityEvent onPlayerBlockedHit;
    public UnityEvent onPlayerTurnStarted;
    public UnityEvent onEnemyTurnStarted;
    public UnityEvent onWaveSpawned;
    public UnityEvent onCombatWon;
    public UnityEvent onCombatLost;
    public UnityEvent onFled;

    public int Sword { get; private set; }
    public int Score { get; private set; }
    public bool PlayerTurnActive { get; private set; }

    public bool DefendActive { get; private set; }   // Defend: halve incoming, this round only
    public int BlockCharges { get; private set; }    // Block: negate next hit, persists until used
    public bool HubrisActive { get; private set; }   // Hubris: no defense cards this round
    public bool AvariceActive { get; private set; }  // Avarice: hand no longer reshuffles, rest of encounter

    public Anim_PlayerAttack PlayerAttackAnim => playerAttackAnim;

    private readonly List<Enemy> enemies = new List<Enemy>();
    private EncounterData encounter;
    private int waveIndex;
    private bool nextWaveIncoming;
    private bool encounterOver = true;

    [Header("Debug")]
    [SerializeField] bool debugOn = false;
    [SerializeField] private EncounterData debugEncounter;

    private void Start()
    {
        Sword = startingSword;
        onSwordChanged.Invoke(Sword);
        hand.onHandEmptied.AddListener(OnHandEmptied);

        if (debugOn)
        {
            encounter = debugEncounter;
            BeginEncounter(encounter);
        }
    }

    public void BeginEncounter(EncounterData data)
    {
        Debug.Log("encounter start");

        ClearEnemies();

        encounter = data;
        waveIndex = 0;
        nextWaveIncoming = false;
        encounterOver = false;

        hand.PlayHandler = TryPlayCard; // combat interprets cards again
        deck.BuildDeck();
        SpawnWave(0);
        StartPlayerTurn();
    }

    private void SpawnWave(int index)
    {
        EncounterData.Wave wave = encounter.waves[index];
        Debug.Log("Spawning wave");
        for (int i = 0; i < wave.enemyPrefabs.Count; i++)
        {
            Debug.Log($"{wave.enemyPrefabs[i]}");

            if (wave.enemyPrefabs[i] == null) continue;

            Transform point = enemySpawnPoints[Mathf.Min(i, enemySpawnPoints.Count - 1)];
            Enemy enemy = Instantiate(wave.enemyPrefabs[i], point.transform.position, Quaternion.identity, enemySpawnParent);
            enemy.transform.localScale = wave.enemyPrefabs[i].transform.localScale;
            enemies.Add(enemy);

            if (HubrisActive)
            {
                enemy.ApplyVulnerable(99);
            }
        }
        onWaveSpawned.Invoke();
    }

    private void ClearEnemies()
    {
        foreach (Enemy enemy in enemies)
            if (enemy != null) Destroy(enemy.gameObject);
        enemies.Clear();
    }

    private void StartPlayerTurn()
    {
        if (encounterOver) return;
        Debug.Log("started player turn");
        PlayerTurnActive = true;
        DrawCards(drawPerTurn);

        if (hand.Count == 0)
        {
            CardData judgement = deck.DrawJudgement();
            if (judgement != null)
                hand.AddCard(judgement);
        }

        onPlayerTurnStarted.Invoke();
    }

    public void DrawCards(int count)
    {
        Debug.Log("Trying to draw");
        for (int i = 0; i < count; i++)
        {
            CardData card = deck.Draw();
            //Debug.Log($"{card.name}");
            if (card == null) break;
            hand.AddCard(card);
        }
    }

    private bool TryPlayCard(CardData data)
    {
        if (!PlayerTurnActive || encounterOver) return false;
        if (!(data is ActionCardData action)) return false;
        if (!action.CanPlay(this)) return false;

        action.Play(this);
        //deck.AddToDiscard(data);
        return true;
    }

    public void PerformPlayerAttack(List<Enemy> targets, int damage, int vulnerableStacks)
    {
        if (targets.Count == 0) return;

        Action impact = () =>
        {
            foreach (Enemy target in targets)
            {
                if (vulnerableStacks > 0)
                    target.ApplyVulnerable(vulnerableStacks);
                if (!target.IsAlive) continue;
                DealDamage(target, damage);
            }
            CheckWaveCleared();
        };

        if (playerAttackAnim != null)
            playerAttackAnim.Play(targets[0].transform.position, impact);
        else
            impact();
    }

    private void OnHandEmptied()
    {
        return;

        if (PlayerTurnActive && !encounterOver)
            EndTurn();
    }

    public void EndTurn()
    {
        if (!PlayerTurnActive || encounterOver) return;
        PlayerTurnActive = false;

        if (!AvariceActive)
        {
            foreach (CardData card in hand.DiscardAllCards())
                deck.AddToDiscard(card);
        }

        StartCoroutine(EnemyPhase());
    }

    private IEnumerator EnemyPhase()
    {
        while (playerAttackAnim != null && playerAttackAnim.IsPlaying)
            yield return null;

        while (nextWaveIncoming)
            yield return null;

        if (encounterOver) yield break;
        onEnemyTurnStarted.Invoke();

        foreach (Enemy enemy in new List<Enemy>(enemies))
        {
            if (encounterOver) yield break;
            if (!enemy.CanAttack) continue;
            if (enemy == null || !enemy.IsAlive) continue;

            yield return new WaitForSeconds(enemyAttackDelay);

            enemy.onAttack.Invoke();

            Action impact = () => ApplyIncomingDamage(enemy.AttackDamage);

            if (enemy.AttackAnim != null)
            {
                Vector3 target = player != null
                    ? player.position
                    : enemy.transform.position;
                yield return enemy.AttackAnim.DamageFeedback(player, impact).WaitForCompletion();
            }
            else
            {
                impact();
            }

            enemy.AttackUpdates();

            Debug.Log($"{enemy.name} did {enemy.AttackDamage} damage");
        }

        if (encounterOver) yield break;

        foreach (Enemy enemy in enemies)
            if (enemy != null && enemy.IsAlive) enemy.TickVulnerable();

        StartPlayerTurn();
    }

    private void ApplyIncomingDamage(int rawDamage)
    {
        if (encounterOver) return;

        int damage = rawDamage;
        if (BlockCharges > 0)
        {
            BlockCharges--;
            onPlayerBlockedHit.Invoke();
            onBlockChanged.Invoke(BlockCharges > 0);
            return;
        }
        if (DefendActive)
            damage /= 2;

        onPlayerDamaged.Invoke(damage);
        ChangeSword(-damage);

        Debug.Log($"Player has {Sword} health");
    }

    public List<Enemy> GetAttackTargets(bool hitAll, bool canHitFlying)
    {
        List<Enemy> targets = new List<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            if (enemy.IsFlying && !canHitFlying) continue;

            targets.Add(enemy);
            if (!hitAll) break;
        }
        return targets;
    }

    public void DealDamage(Enemy target, int amount)
    {
        int final = target.VulnerableStacks > 0 ? Mathf.RoundToInt(amount * 2.0f) : amount;
        Debug.Log($"{target.name} took {final} damage");
        target.TakeDamage(final);
    }

    public void ActivateDefend()
    {
        DefendActive = true;
        onDefendChanged.Invoke(DefendActive);
    }
    public void AddBlockCharge()
    {
        BlockCharges++;
        onBlockChanged.Invoke(BlockCharges > 0);
    }

    public void ActivateHubris()
    {
        HubrisActive = true;
        DefendActive = false;
        BlockCharges = 0;
        onDefendChanged.Invoke(DefendActive);
        onBlockChanged.Invoke(BlockCharges > 0);
    }

    public void ActivateAvarice() => AvariceActive = true;

    public void ChangeSword(int delta)
    {
        Sword = Mathf.Max(0, Sword + delta);
        onSwordChanged.Invoke(Sword);
        if (Sword <= 0 && !encounterOver)
            Lose();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        onScoreChanged.Invoke(Score);
    }

    public void RebuildHand()
    {
        deck.BuildDeck();
    }

    public void Flee()
    {
        if (encounterOver) return;
        encounterOver = true;
        PlayerTurnActive = false;
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        enemies.Clear();
        CleanUpDeckAfterEncounter();
        onFled.Invoke();
    }

    public void AcceptJudgement()
    {
        AddScore(Score);
        ChangeSword(-Sword);
    }

    private void CheckWaveCleared()
    {
        if (encounterOver || nextWaveIncoming) return;

        foreach (Enemy enemy in enemies)
            if (enemy != null && enemy.IsAlive) return;

        if (waveIndex + 1 < encounter.waves.Count)
        {
            StartCoroutine(SpawnNextWave());
            return;
        }

        Win();
    }

    private IEnumerator SpawnNextWave()
    {
        nextWaveIncoming = true;
        yield return new WaitForSeconds(encounter.delayBetweenWaves);
        waveIndex++;
        SpawnWave(waveIndex);
        nextWaveIncoming = false;
    }

    private void Win()
    {
        encounterOver = true;
        PlayerTurnActive = false;
        AddScore(encounter.bounty);
        CleanUpDeckAfterEncounter();
        onCombatWon.Invoke();
    }

    private void Lose()
    {
        encounterOver = true;
        PlayerTurnActive = false;
        onCombatLost.Invoke();
    }

    private void CleanUpDeckAfterEncounter()
    {
        foreach (CardData card in hand.DiscardAllCards())
            deck.ReturnToBottom(card);
        deck.EndEncounter();
        AvariceActive = false;
        BlockCharges = 0;
        DefendActive = false;
        HubrisActive = false;
        onDefendChanged.Invoke(DefendActive);
        onBlockChanged.Invoke(BlockCharges > 0);
    }
}
