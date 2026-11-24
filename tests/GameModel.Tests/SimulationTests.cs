using GameModel.Battle;
using GameModel.Core;
using GameModel.Data;
using GameModel.Simulation;
using GameModel.Units;
using static GameModel.Tests.TestData;

namespace GameModel.Tests;

public class SimulationTests
{
    private static (StageDef stage, List<UnitDef> squad) ForestRoad()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(dir!.FullName, "data"))) dir = dir.Parent;
        string data = Path.Combine(dir.FullName, "data");
        var champs = DataLoader.ParseChampions(File.ReadAllText(Path.Combine(data, "champions.json")));
        var file = DataLoader.ParseStage(File.ReadAllText(Path.Combine(data, "stage-forest-road.json")));
        return (file.Stage, DataLoader.ResolveSquad(file, champs));
    }

    [Fact]
    public void SameSeedPlaysTheSameBattle()
    {
        var (stage, squad) = ForestRoad();
        var a = BattleSimulator.Run(new GameModel.Battle.Battle(stage, squad, new SeededRandom(3)));
        var b = BattleSimulator.Run(new GameModel.Battle.Battle(stage, squad, new SeededRandom(3)));
        Assert.Equal(a.Outcome, b.Outcome);
        Assert.Equal(a.Seconds, b.Seconds);
        Assert.Equal(a.Stats.DamageDealt, b.Stats.DamageDealt);
    }

    [Fact]
    public void AutoPlayWinsTheSampleStageMostOfTheTime()
    {
        var (stage, squad) = ForestRoad();
        int wins = 0;
        for (int seed = 0; seed < 50; seed++)
        {
            var r = BattleSimulator.Run(new GameModel.Battle.Battle(stage, squad, new SeededRandom(seed)));
            if (r.Outcome == BattlePhase.Won) wins++;
        }
        Assert.InRange(wins, 35, 50);
    }

    [Fact]
    public void AutoPlayLandsSparksAndEmpowers()
    {
        var (stage, squad) = ForestRoad();
        var r = BattleSimulator.Run(new GameModel.Battle.Battle(stage, squad, new SeededRandom(1)));
        Assert.True(r.Stats.Sparks > 0);
        Assert.True(r.Stats.Empowers > 0);
    }

    [Fact]
    public void HopelessFightIsLost()
    {
        var stage = Stage(Wave(Enemy(hp: 1_000_000, atk: 5000)));
        var r = BattleSimulator.Run(new GameModel.Battle.Battle(stage, [Champion()], new SeededRandom(0)));
        Assert.Equal(BattlePhase.Lost, r.Outcome);
    }

    [Fact]
    public void TimeLimitCountsAsALoss()
    {
        var stage = Stage(Wave(Enemy(hp: 1_000_000, atk: 1)));
        var r = BattleSimulator.Run(new GameModel.Battle.Battle(stage, [Champion(atk: 10)], new SeededRandom(0)), timeLimit: 30);
        Assert.Equal(BattlePhase.Lost, r.Outcome);
    }
}
