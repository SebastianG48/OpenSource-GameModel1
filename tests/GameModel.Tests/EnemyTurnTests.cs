using GameModel.Battle;
using GameModel.Core;
using static GameModel.Tests.TestData;

namespace GameModel.Tests;

public class EnemyTurnTests
{
    [Fact]
    public void EnemiesWaitForTheirTimer()
    {
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy())), [Champion()], new FixedRandom(0.5));
        b.Start();
        Assert.Empty(b.Tick(1.0));
        Assert.NotEmpty(b.Tick(b.Tuning.EnemyIntervalMax));
    }

    [Fact]
    public void EnemyHitsLowerChampionHp()
    {
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy(atk: 60, element: GameModel.Elements.Element.Light))), [Champion()], new FixedRandom(0.5));
        b.Start();
        var hits = b.Tick(10);
        // 60 ATK - 20 DEF * 0.5 = 50
        Assert.Equal(50, hits.Single().Damage);
        Assert.Equal(950, b.Champions[0].Hp);
    }

    [Fact]
    public void EnemyHitsDoNotFillEnemyGauges()
    {
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy())), [Champion()], new FixedRandom(0.5));
        b.Start();
        Assert.Equal(0, b.Tick(10).Single().Crystals);
    }

    [Fact]
    public void LosingEveryChampionLosesTheBattle()
    {
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy(atk: 5000))), [Champion(hp: 100)], new FixedRandom(0.5));
        b.Start();
        b.Tick(10);
        Assert.Equal(BattlePhase.Lost, b.Phase);
        Assert.Empty(b.Tick(20));
    }
}
