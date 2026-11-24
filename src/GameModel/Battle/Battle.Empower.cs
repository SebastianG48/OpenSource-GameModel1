using GameModel.Units;

namespace GameModel.Battle;

public sealed partial class Battle
{
    /// <summary>True when this champion's gauge is full and it is free to act.</summary>
    public bool CanEmpower(Unit champion, double now) =>
        Fighting && champion.Side == Side.Champion && champion.Alive && champion.GaugeFull;

    /// <summary>
    /// Uses the champion's Empower. Empowers skip the attack cooldown (a full gauge is reward enough),
    /// empty the gauge, and give back only a quarter of the usual crystals.
    /// </summary>
    public IReadOnlyList<HitResult> UseEmpower(Unit champion, Unit? target, double now)
    {
        if (!CanEmpower(champion, now)) return [];
        var emp = champion.Def.Empower!;
        champion.Gauge = 0;
        Write(now, $"{champion.Name} uses {emp.Name}!");

        var results = new List<HitResult>();
        if (emp.Target == EmpowerTarget.AllAllies)
        {
            results.AddRange(HealSquad(champion, emp, now));
        }
        else
        {
            var targets = emp.Target == EmpowerTarget.AllEnemies
                ? AliveEnemies.ToList()
                : new List<Unit> { target is { Alive: true } ? target : DefaultTarget(champion)! };

            for (int i = 0; i < emp.HitWeights.Length; i++)
            {
                double time = now + i * Tuning.HitSpacing;
                foreach (var t in targets.Where(t => t is { Alive: true }))
                    results.Add(ResolveHit(champion, t, emp.Power * emp.HitWeights[i], time, empower: true));
            }

            if (emp.HealFraction > 0) results.AddRange(HealSquad(champion, emp, now));
        }

        AfterAction(now);
        return results;
    }

    private IEnumerable<HitResult> HealSquad(Unit healer, EmpowerDef emp, double now)
    {
        foreach (var c in AliveChampions.ToList())
        {
            int amount = (int)Math.Round(c.MaxHp * emp.HealFraction + healer.Def.Rec);
            int healed = c.Heal(amount);
            yield return new HitResult { Attacker = healer, Target = c, Time = now, Healed = healed };
        }
    }
}
