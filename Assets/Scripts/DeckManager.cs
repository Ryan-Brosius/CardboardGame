using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeckManager : MonoBehaviour
{
    // Maybe a bad name? This really is the "COMBAT" deck. Maybe requires a change later, and shuffle gets moved into a UTIL

    [SerializeField] private List<CardData> startingDeck = new List<CardData>();
    [SerializeField] private CardData JusticeCard;
    [SerializeField] private CardData ConclusionCard;

    public UnityEvent<int> onDrawPileChanged;
    public UnityEvent<int> onDiscardPileChanged;
    public UnityEvent<int> onOwnedCardsChanged;

    private readonly List<CardData> ownedCards = new List<CardData>();
    private readonly List<CardData> drawPile = new List<CardData>();    // 0 = top
    private readonly List<CardData> discardPile = new List<CardData>();

    public IReadOnlyList<CardData> OwnedCards => ownedCards;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public List<CardData> DrawPile => drawPile;

    public bool JudgementSpawned { get; private set; }

    // REMOVE LATER TESTING
    //private void Awake()
    //{
    //    InitializeRun();
    //}

    public void InitializeRun()
    {
        ownedCards.Clear();
        ownedCards.AddRange(startingDeck);
        onOwnedCardsChanged.Invoke(ownedCards.Count);
    }

    public void AddOwnedCard(CardData card)
    {
        if (card == null) return;
        ownedCards.Add(card);
        onOwnedCardsChanged.Invoke(ownedCards.Count);
    }

    public bool RemoveOwnedCard(CardData card)
    {
        bool removed = ownedCards.Remove(card);
        if (removed) onOwnedCardsChanged.Invoke(ownedCards.Count);
        return removed;
    }

    public void RemoveCowardiceFromDraw(CardData card)
    {
        drawPile.Remove(card);
    }

    public void BuildDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        JudgementSpawned = false;
        drawPile.AddRange(ownedCards);
        Shuffle();
        NotifyChanged();
    }

    public CardData Draw()
    {
        if (drawPile.Count == 0)
            ReshuffleDiscardPile();

        if (drawPile.Count == 0)
            return null;

        CardData card = drawPile[0];
        drawPile.RemoveAt(0);
        NotifyChanged();
        return card;
    }

    public CardData DrawJudgement()
    {
        if (JudgementSpawned || JusticeCard == null) return null;
        JudgementSpawned = true;
        return JusticeCard;
    }

    public CardData DrawConclusion()
    {
        if (ConclusionCard != null) return ConclusionCard;
        else return null;
    }

    public void ReturnToBottom(CardData card)
    {
        int insertAt = drawPile.Count;
        while (insertAt > 0 && drawPile[insertAt - 1].alwaysLastInDeck)
            insertAt--;
        drawPile.Insert(insertAt, card);
        NotifyChanged();
    }

    public void AddToDiscard(CardData card)
    {
        discardPile.Add(card);
        NotifyChanged();
    }

    public void EndEncounter()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle();
        NotifyChanged();
    }

    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0) return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle();
        NotifyChanged();
    }

    private void Shuffle()
    {
        List<CardData> pinned = new List<CardData>();
        for (int i = drawPile.Count - 1; i >= 0; i--)
        {
            if (drawPile[i] != null && drawPile[i].alwaysLastInDeck)
            {
                pinned.Add(drawPile[i]);
                drawPile.RemoveAt(i);
            }
        }

        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }

        drawPile.AddRange(pinned);
    }

    private void NotifyChanged()
    {
        onDrawPileChanged.Invoke(drawPile.Count);
        onDiscardPileChanged.Invoke(discardPile.Count);
    }
}
