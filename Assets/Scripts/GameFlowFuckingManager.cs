using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// TLDR: director fucker
public class GameFlowFuckingManager : MonoBehaviour
{
    public enum Phase { Combat, Spoils, Almonry, ZoneSelect, GameOver }

    [Header("References")]
    [SerializeField] private HandController hand;
    [SerializeField] private DeckManager deck;
    [SerializeField] private CombatManager combat;

    [Header("Run")]
    [SerializeField] private EncounterData firstEncounter;
    [Tooltip("Beat after combat ends before the next phase's cards appear.")]
    [SerializeField] private float phaseTransitionDelay = 0.8f;

    [Header("Spoils of War")]
    [Tooltip("'Add a card to your deck' sign object — enabled during this phase.")]
    [SerializeField] private GameObject spoilsSign;
    [Tooltip("Cards that can be offered as rewards.")]
    [SerializeField] private List<ActionCardData> rewardPool = new List<ActionCardData>();
    [SerializeField] private int spoilsChoices = 4;

    [Header("The Almonry")]
    [Tooltip("'Donate a card to restore balance' sign object.")]
    [SerializeField] private GameObject almonrySign;
    [SerializeField] private int almonryChoices = 4;
    [Tooltip("Sword raise for a normal donation. Multiplied by the card's donationMultiplier (Temperance = 3).")]
    [SerializeField] private int donationSwordRaise = 25;

    [Header("Zone Select")]
    [Tooltip("'Choose your path' sign object.")]
    [SerializeField] private GameObject zoneSign;
    [Tooltip("Zone cards that can be offered. For now they're all combat encounters.")]
    [SerializeField] private List<ZoneCardData> zonePool = new List<ZoneCardData>();
    [SerializeField] private int zoneChoices = 2;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverSign;

    [Header("Events")]
    public UnityEvent<string> onPhaseChanged;

    public Phase CurrentPhase { get; private set; }
    private bool transitioning;

    private void Start()
    {
        combat.onCombatWon.AddListener(OnCombatWon);
        combat.onCombatLost.AddListener(OnCombatLost);
        combat.onFled.AddListener(OnCombatFled);

        deck.InitializeRun();
        StartCombat(firstEncounter);
    }

    private void SetPhase(Phase phase)
    {
        CurrentPhase = phase;
        if (spoilsSign != null) spoilsSign.SetActive(phase == Phase.Spoils);
        if (almonrySign != null) almonrySign.SetActive(phase == Phase.Almonry);
        if (zoneSign != null) zoneSign.SetActive(phase == Phase.ZoneSelect);
        if (gameOverSign != null) gameOverSign.SetActive(phase == Phase.GameOver);
        onPhaseChanged.Invoke(phase.ToString());
    }

    private void StartCombat(EncounterData encounter)
    {
        SetPhase(Phase.Combat);
        combat.BeginEncounter(encounter);
    }

    private void OnCombatWon() => StartCoroutine(AfterDelay(StartSpoils));
    private void OnCombatFled() => StartCoroutine(AfterDelay(StartZoneSelect));
    private void OnCombatLost() => GameOver();

    private void StartSpoils()
    {
        SetPhase(Phase.Spoils);
        hand.PlayHandler = OnSpoilsCardPlayed;
        DealRandom(rewardPool, spoilsChoices);
    }

    private bool OnSpoilsCardPlayed(CardData card)
    {
        if (transitioning || CurrentPhase != Phase.Spoils) return false;

        deck.AddOwnedCard(card);
        StartCoroutine(FinishPickThen(StartAlmonry));
        return true;
    }

    private void StartAlmonry()
    {
        SetPhase(Phase.Almonry);

        List<CardData> donatable = new List<CardData>();
        foreach (CardData card in deck.OwnedCards)
            if (card != null && !card.cannotBeDonated)
                donatable.Add(card);

        if (donatable.Count == 0)
        {
            StartZoneSelect();
            return;
        }

        hand.PlayHandler = OnAlmonryCardPlayed;
        DealRandom(donatable, almonryChoices);
    }

    private bool OnAlmonryCardPlayed(CardData card)
    {
        if (transitioning || CurrentPhase != Phase.Almonry) return false;
        if (!deck.RemoveOwnedCard(card)) return false;

        combat.ChangeSword(donationSwordRaise * Mathf.Max(1, card.donationMultiplier));
        StartCoroutine(FinishPickThen(StartZoneSelect));
        return true;
    }

    private void StartZoneSelect()
    {
        SetPhase(Phase.ZoneSelect);
        hand.PlayHandler = OnZoneCardPlayed;
        DealRandom(zonePool, zoneChoices);
    }

    private bool OnZoneCardPlayed(CardData card)
    {
        if (transitioning || CurrentPhase != Phase.ZoneSelect) return false;
        if (!(card is ZoneCardData zone) || zone.encounter == null) return false;

        StartCoroutine(FinishPickThen(() => StartCombat(zone.encounter)));
        return true;
    }

    private void GameOver()
    {
        SetPhase(Phase.GameOver);
        hand.PlayHandler = null;
        hand.DiscardAllCards();
    }

    private void DealRandom<T>(List<T> pool, int count) where T : CardData
    {
        List<T> candidates = new List<T>(pool);
        candidates.RemoveAll(c => c == null);

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int deal = Mathf.Min(count, candidates.Count);
        for (int i = 0; i < deal; i++)
            hand.AddCard(candidates[i]);
    }

    private IEnumerator FinishPickThen(Action nextPhase)
    {
        transitioning = true;
        yield return null;
        hand.DiscardAllCards();
        yield return new WaitForSeconds(phaseTransitionDelay);
        transitioning = false;
        nextPhase();
    }

    private IEnumerator AfterDelay(Action next)
    {
        yield return new WaitForSeconds(phaseTransitionDelay);
        next();
    }
}
