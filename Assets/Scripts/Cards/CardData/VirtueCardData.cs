using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Virtue Card", fileName = "VirtueCard")]
public class VirtueCardData : ActionCardData
{
    public enum VirtueType
    {
        Hubris,
        Temperance,  
        Avarice,     
        Indulgence,  
        Cowardice,   
        Justice,
        Conclusion
    }

    [Header("Virtue")]
    public VirtueType type;
    [Tooltip("Change applied to the Sword of Damocles when played (negative lowers it).")]
    public int swordChange;
    [Tooltip("Hubris: vulnerable stacks applied to every enemy.")]
    public int vulnerableStacks = 2;
    [Tooltip("Indulgence: how many cards to draw.")]
    public int cardsToDraw = 2;

    public override bool CanPlay(CombatManager combat)
    {
        return type != VirtueType.Temperance;
    }

    public override void Play(CombatManager combat)
    {
        switch (type)
        {
            case VirtueType.Hubris:
                foreach (Enemy e in combat.GetAttackTargets(hitAll: true, canHitFlying: true))
                    e.ApplyVulnerable(vulnerableStacks);
                combat.ActivateHubris();
                break;

            case VirtueType.Avarice:
                combat.ActivateAvarice();
                combat.ChangeSword(swordChange);
                break;

            case VirtueType.Indulgence:
                combat.DrawCards(cardsToDraw);
                combat.ChangeSword(swordChange);
                break;

            case VirtueType.Cowardice:
                combat.ChangeSword(swordChange);
                combat.RebuildHand(this);
                break;

            case VirtueType.Justice:
                combat.AcceptJudgement();
                break;
            case VirtueType.Conclusion:
                combat.EndTurn();
                break;
        }
    }
}
