using GameModel.Battle;

namespace GameModel.Simulation;

/// <summary>The outcome of a simulated battle.</summary>
public sealed record SimulationResult(BattlePhase Outcome, double Seconds, BattleStats Stats, int SurvivingChampions);

/// <summary>
/// Runs a battle to the end with <see cref="AutoPlayer"/>, stepping time in fixed slices. Used for
/// balance checks: run a stage a few hundred times with different seeds and look at the win rate.
/// </summary>
public static class BattleSimulator
{
    public static SimulationResult Run(Battle.Battle battle, double step = 0.05, double timeLimit = 600)
    {
        if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
        var auto = new AutoPlayer();
        double now = 0;
        battle.Start(now);

        while (battle.Fighting && now < timeLimit)
        {
            auto.Act(battle, now);
            battle.Tick(now);
            now += step;
        }

        // Running out of time counts as a loss.
        var outcome = battle.Fighting ? BattlePhase.Lost : battle.Phase;
        return new SimulationResult(outcome, Math.Round(now, 2), battle.Stats, battle.AliveChampions.Count());
    }
}
