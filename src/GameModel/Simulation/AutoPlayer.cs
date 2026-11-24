using GameModel.Units;

namespace GameModel.Simulation;

/// <summary>
/// The game's Auto button: decides what each champion does. It mirrors how a good player taps:
/// <list type="number">
/// <item>Heal first when the squad is hurt and a healer is ready.</item>
/// <item>Fire any damage Empower that is ready.</item>
/// <item>Otherwise, have every ready champion attack the same target at the same moment, so their
/// hits land together and Spark.</item>
/// </list>
/// </summary>
public sealed class AutoPlayer
{
    /// <summary>Squad HP share below which a healing Empower is used.</summary>
    public float HealBelow { get; init; } = 0.6f;

    public void Act(Battle.Battle battle, double now)
    {
        if (!battle.Fighting) return;

        foreach (var c in battle.AliveChampions.ToList())
        {
            if (!battle.CanEmpower(c, now)) continue;
            bool healer = c.Def.Empower!.Target == EmpowerTarget.AllAllies;
            if (healer && SquadHpShare(battle) >= HealBelow) continue;
            battle.UseEmpower(c, null, now);
        }

        // Everyone who is ready hits one shared target, so the hits overlap and Spark.
        var ready = battle.AliveChampions.Where(c => battle.CanAttack(c, now)).ToList();
        if (ready.Count == 0) return;
        var focus = battle.DefaultTarget(ready[0]);
        foreach (var c in ready) battle.Attack(c, focus, now);
    }

    private static float SquadHpShare(Battle.Battle b)
    {
        int max = b.Champions.Sum(c => c.MaxHp);
        return max == 0 ? 0 : (float)b.Champions.Sum(c => c.Hp) / max;
    }
}
