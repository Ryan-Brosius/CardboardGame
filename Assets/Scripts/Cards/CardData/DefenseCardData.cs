using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense Card", fileName = "DefenseCard")]
public class DefenseCardData : ActionCardData
{
    public enum DefenseType
    {
        HalveIncomingThisRound,
        NegateNextHit,
    }

    [Header("Defense")]
    public DefenseType type = DefenseType.HalveIncomingThisRound;

    public override bool CanPlay(CombatManager combat)
    {
        return !combat.HubrisActive;
    }

    public override void Play(CombatManager combat)
    {
        switch (type)
        {
            case DefenseType.HalveIncomingThisRound:
                combat.ActivateDefend();
                break;
            case DefenseType.NegateNextHit:
                combat.AddBlockCharge();
                break;
        }
    }
}
