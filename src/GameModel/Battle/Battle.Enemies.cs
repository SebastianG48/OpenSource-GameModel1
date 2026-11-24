using GameModel.Units;

namespace GameModel.Battle;

public sealed partial class Battle
{
    /// <summary>
    /// Advances the battle to <paramref name="now"/>: every enemy whose timer is up attacks.
    /// Call it every frame (or every simulation step).
    /// </summary>
    public IReadOnlyList<HitResult> Tick(double now)
    {
        if (!Fighting) return [];
        var results = new List<HitResult>();

        foreach (var enemy in AliveEnemies.ToList())
        {
            if (!Fighting) break;
            if (now < enemy.NextActionAt || enemy.IsFrozen(now)) continue;

            var target = PickEnemyTarget();
            if (target == null) break;

            foreach (var w in enemy.Def.HitWeights)
            {
                if (!target.Alive) break;
                results.Add(ResolveHit(enemy, target, w, now, empower: false));
            }
            enemy.NextActionAt = now + EnemyInterval(enemy);
            AfterAction(now);
        }

        return results;
    }

    /// <summary>Enemies pick a random living champion, so no one champion soaks every hit.</summary>
    private Unit? PickEnemyTarget()
    {
        var alive = AliveChampions.ToList();
        return alive.Count == 0 ? null : alive[_rng.Range(0, alive.Count - 1)];
    }
}
