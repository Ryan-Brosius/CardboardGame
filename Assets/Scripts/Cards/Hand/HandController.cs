using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// I dont care no commends because game jam :)
// Ryan made this its to display the hand
public class HandController : MonoBehaviour
{
    [SerializeField] private HandSettings handSettings;
    [SerializeField] private CardSettings cardSettings;
    [SerializeField] private CardView cardPrefab;

    [Header("Debug")]
    [SerializeField] private bool enableDebugSpawn = false;
    [SerializeField] private int startingCards = 0;

    [Header("Events")]
    public UnityEvent onHandEmptied;

    // Pass the function to play the current cards in hand here
    // maybe change later to be less confusing?
    public Func<CardData, bool> PlayHandler;

    private readonly List<CardView> cards = new List<CardView>();
    public IReadOnlyList<CardView> Cards => cards;
    public int Count => cards.Count;

    private void Start()
    {
        if (enableDebugSpawn)
        {
            for (int i = 0; i < startingCards; i++)
                AddCard((CardData)null);
        }

        ApplyLayout(snap: true);
    }

    private void LateUpdate()
    {
        ApplyLayout(snap: false);
    }

    public CardView AddCard(CardData data)
    {
        CardView card = Instantiate(cardPrefab, transform);
        card.Init(this, cardSettings);
        card.SetData(data);
        cards.Add(card);
        ApplyLayout(snap: true);
        return card;
    }

    public void RemoveCard(CardView card)
    {
        if (cards.Remove(card))
        {
            Destroy(card.gameObject);
            ApplyLayout(snap: false);
        }
    }

    public List<CardData> RemoveAllCards()
    {
        List<CardData> datas = new List<CardData>();
        foreach (CardView card in cards)
        {
            if (card.Data != null) datas.Add(card.Data);
            Destroy(card.gameObject);
        }
        cards.Clear();
        return datas;
    }

    public void RequestPlay(CardView card)
    {
        if (PlayHandler == null || card.Data == null) return;
        if (!PlayHandler(card.Data)) return;

        RemoveCard(card);
        if (cards.Count == 0)
            onHandEmptied.Invoke();
    }

    public void ApplyLayout(bool snap)
    {
        int count = cards.Count;
        if (count == 0 || handSettings == null) return;

        float spread = Mathf.Min((count - 1) * handSettings.anglePerCard, handSettings.maxFanAngle);

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t);
            float rad = angle * Mathf.Deg2Rad;

            Vector2 position = handSettings.centerOffset
                + new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * handSettings.fanRadius
                - new Vector2(0f, handSettings.fanRadius);

            float rotation = -angle * handSettings.rotationMultiplier;

            cards[i].SetSlot(i, position, rotation);
            if (snap) cards[i].SnapToSlot();
        }
    }

    public void RestoreDrawOrder()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].IsElevated) continue;
            cards[i].transform.SetSiblingIndex(i);
        }
    }

    private void OnDrawGizmos()
    {
        if (handSettings == null)
            return;

        Handles.color = Color.cyan;
        Vector3 center = transform.position +
                         (Vector3)handSettings.centerOffset -
                         Vector3.up * handSettings.fanRadius;
        Handles.DrawWireDisc(center, Vector3.forward, handSettings.fanRadius);
    }
}
