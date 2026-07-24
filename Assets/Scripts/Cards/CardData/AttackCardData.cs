using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Attack Card", fileName = "AttackCard")]
public class AttackCardData : ActionCardData
{
    [Header("Attack")]
    public int damage = 6;
    public bool hitsAllEnemies;
    public bool canHitFlying;
    public int vulnerableStacks;

    public override bool CanPlay(CombatManager combat)
    {
        return combat.GetAttackTargets(hitsAllEnemies, canHitFlying).Count > 0;
    }

    public override void Play(CombatManager combat)
    {
        foreach (Enemy target in combat.GetAttackTargets(hitsAllEnemies, canHitFlying))
        {
            combat.DealDamage(target, damage);
            if (vulnerableStacks > 0)
                target.ApplyVulnerable(vulnerableStacks);
        }
    }
}
