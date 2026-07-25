using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEngine.GraphicsBuffer;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandController hand;
    [SerializeField] private DeckManager deck;
    [SerializeField] private Transform player;

    [SerializeField] private List<Enemy> enemies = new List<Enemy>();

    [Tooltip("Animations")]
    [SerializeField] private Anim_PlayerAttack playerAttackAnim;

    [Header("Combat Values")]
    [SerializeField] private int startingSword = 100;   // basically just a test for health move later
    [SerializeField] private int drawPerTurn = 3;
    [SerializeField] private int bountyOnWin = 100;
    [SerializeField] private float enemyAttackDelay = 0.6f;

    [Header("Events")]
    public UnityEvent<int> onSwordChanged;
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<int> onPlayerDamaged;
    public UnityEvent onPlayerBlockedHit;
    public UnityEvent onPlayerTurnStarted;
    public UnityEvent onEnemyTurnStarted;
    public UnityEvent onCombatWon;
    public UnityEvent onCombatLost;
    public UnityEvent onFled;

    public int Sword { get; private set; }
    public bool PlayerTurnActive { get; private set; }

    public bool DefendActive { get; private set; }   // Defend: halve incoming, this round only
    public int BlockCharges { get; private set; }    // Block: negate next hit, persists until used
    public bool HubrisActive { get; private set; }   // Hubris: no defense cards this round
    public bool AvariceActive { get; private set; }  // Avarice: hand no longer reshuffles, rest of encounter
    public Anim_PlayerAttack PlayerAttackAnim => playerAttackAnim;

    private bool encounterOver;

    private void Start()
    {
        Sword = startingSword;
        onSwordChanged.Invoke(Sword);

        hand.PlayHandler = TryPlayCard;
        hand.onHandEmptied.AddListener(OnHandEmptied);

        deck.BuildDeck();
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        if (encounterOver) return;
        PlayerTurnActive = true;
        DrawCards(drawPerTurn);
        onPlayerTurnStarted.Invoke();
    }

    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardData card = deck.Draw();
            if (card == null) break;
            hand.AddCard(card);
        }
    }

    private bool TryPlayCard(CardData data)
    {
        if (!PlayerTurnActive || encounterOver) return false;
        if (!(data is ActionCardData action)) return false;
        if (!action.CanPlay(this)) return false;

        Debug.Log($"Combat: player played {data.name} card.");
        action.Play(this);
        deck.AddToDiscard(data);
        CheckVictory();
        return true;
    }

    public void PerformPlayerAttack(List<Enemy> targets, int damage, int vulnerableStacks)
    {
        if (targets.Count == 0) return;

        Action impact = () =>
        {
            foreach (Enemy target in targets)
            {
                if (!target.IsAlive) continue;
                DealDamage(target, damage);
                if (vulnerableStacks > 0)
                    target.ApplyVulnerable(vulnerableStacks);
            }
            CheckVictory();
        };

        if (playerAttackAnim != null)
            playerAttackAnim.Play(targets[0].transform.position, impact);
        else
            impact();
    }

    private void OnHandEmptied()
    {
        if (PlayerTurnActive && !encounterOver)
            EndTurn();
    }

    public void EndTurn()
    {
        if (!PlayerTurnActive || encounterOver) return;
        PlayerTurnActive = false;

        if (!AvariceActive)
        {
            foreach (CardData card in hand.RemoveAllCards())
                deck.ReturnToBottom(card);
        }

        StartCoroutine(EnemyPhase());
    }

    private IEnumerator EnemyPhase()
    {
        while (playerAttackAnim != null && playerAttackAnim.IsPlaying)
            yield return null;

        onEnemyTurnStarted.Invoke();

        foreach (Enemy enemy in enemies)
        {
            if (encounterOver) yield break;
            if (!enemy.IsAlive) continue;

            yield return new WaitForSeconds(enemyAttackDelay);

            enemy.onAttack.Invoke();

            Action impact = () => ApplyIncomingDamage(enemy.AttackDamage);

            if (enemy.AttackAnim != null)
            {
                Vector3 target = player.position;

                yield return enemy.AttackAnim.DamageFeedback(impact).WaitForCompletion();
            }
            else
            {
                impact();
            }
        }

        if (encounterOver) yield break;

        DefendActive = false;
        HubrisActive = false;
        foreach (Enemy enemy in enemies)
            if (enemy.IsAlive) enemy.TickVulnerable();

        Debug.Log($"Player has {Sword} health left");
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
            return;
        }
        if (DefendActive)
            damage /= 2;

        onPlayerDamaged.Invoke(damage);
        ChangeSword(-damage);
    }

    public List<Enemy> GetAttackTargets(bool hitAll, bool canHitFlying)
    {
        List<Enemy> targets = new List<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            if (enemy.IsFlying && !canHitFlying) continue;

            targets.Add(enemy);
            if (!hitAll) break;
        }
        return targets;
    }

    public void DealDamage(Enemy target, int amount)
    {
        int final = target.VulnerableStacks > 0 ? Mathf.RoundToInt(amount * 1.5f) : amount;
        target.TakeDamage(final);
        Debug.Log($"Player did {final} damage to {target.EnemyName} leaving them with {target.Health} health");
    }

    public void ActivateDefend() => DefendActive = true;
    public void AddBlockCharge() => BlockCharges++;

    public void ActivateHubris()
    {
        HubrisActive = true;
        DefendActive = false;
        BlockCharges = 0;
    }

    public void ActivateAvarice() => AvariceActive = true;

    public void ChangeSword(int delta)
    {
        Sword = Mathf.Max(0, Sword + delta);
        onSwordChanged.Invoke(Sword);
        if (Sword <= 0 && !encounterOver)
            Lose();
    }

    public void Flee()
    {
        if (encounterOver) return;
        encounterOver = true;
        PlayerTurnActive = false;
        CleanUpDeckAfterEncounter();
        onFled.Invoke();
    }

    public void AcceptJudgement()
    {
        ChangeSword(-Sword);
    }

    private void CheckVictory()
    {
        foreach (Enemy enemy in enemies)
            if (enemy.IsAlive) return;

        encounterOver = true;
        PlayerTurnActive = false;
        CleanUpDeckAfterEncounter();
        onCombatWon.Invoke();
    }

    private void Lose()
    {
        encounterOver = true;
        PlayerTurnActive = false;
        onCombatLost.Invoke();
        Debug.Log("Player Lost");
    }

    private void CleanUpDeckAfterEncounter()
    {
        foreach (CardData card in hand.RemoveAllCards())
            deck.ReturnToBottom(card);
        deck.EndEncounter();
        AvariceActive = false;
        BlockCharges = 0;
        DefendActive = false;
        HubrisActive = false;
    }
}
