using UnityEngine;

public abstract class CardData : ScriptableObject
{
    [Header("Display")]
    public string title;
    public Sprite cardArt;
    [TextArea] public string description;

    [Header("Deck Rules")]
    public bool alwaysLastInDeck;
    public bool isConclusionCard;

    [Header("Almonry")]
    public bool cannotBeDonated;
    public int donationMultiplier = 1;
}

// Defines the cards that can be played in combat
public abstract class ActionCardData : CardData
{
    public virtual bool CanPlay(CombatManager combat) => true;

    public abstract void Play(CombatManager combat);
}


/********************************************
 * 
 *  Note:
 *  Define more classes like above when you create
 *  more complex behaviors like title screen,
 *  traveling, shop... etc
 * 
 * ******************************************/