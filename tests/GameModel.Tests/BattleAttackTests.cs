using GameModel.Battle;
using GameModel.Core;
using GameModel.Elements;
using static GameModel.Tests.TestData;

namespace GameModel.Tests;

public class BattleAttackTests
{
    private static GameModel.Battle.Battle NewBattle(StageDef stage, params GameModel.Units.UnitDef[] squad)
    {
        var b = new GameModel.Battle.Battle(stage, squad, new FixedRandom(0.5));
        b.Start();
        return b;
    }

    [Fact]
    public void AttackDamagesTheTarget()
    {
        var b = NewBattle(Stage(Wave(Enemy(element: Element.Light))), Champion());
        var hits = b.Attack(b.Champions[0], null, 0);
        Assert.Single(hits);
        Assert.Equal(100, hits[0].Damage);
        Assert.Equal(400, b.Enemies[0].Hp);
    }

    [Fact]
    public void ChampionMustWaitForCooldown()
    {
        var b = NewBattle(Stage(Wave(Enemy())), Champion());
        var knight = b.Champions[0];
        Assert.NotEmpty(b.Attack(knight, null, 0));
        Assert.Empty(b.Attack(knight, null, 1.0));
        Assert.NotEmpty(b.Attack(knight, null, b.Tuning.ChampionCooldown));
    }

    [Fact]
    public void DefaultTargetPrefersElementAdvantage()
    {
        // Water beats Fire
        var b = NewBattle(Stage(Wave(Enemy("Rock", Element.Earth, hp: 100), Enemy("Imp", Element.Fire, hp: 900))),
            Champion(element: Element.Water));
        Assert.Equal("Imp", b.DefaultTarget(b.Champions[0])!.Name);
    }

    [Fact]
    public void TwoChampionsHittingTogetherMakeASpark()
    {
        var b = NewBattle(Stage(Wave(Enemy(hp: 5000, element: Element.Light))), Champion("A"), Champion("B"));
        var first = b.Attack(b.Champions[0], null, 0.0);
        var second = b.Attack(b.Champions[1], null, 0.1);
        Assert.False(first[0].Spark);
        Assert.True(second[0].Spark);
        Assert.Equal(150, second[0].Damage);
    }

    [Fact]
    public void SlowFollowUpIsNotASpark()
    {
        var b = NewBattle(Stage(Wave(Enemy(hp: 5000))), Champion("A"), Champion("B"));
        b.Attack(b.Champions[0], null, 0.0);
        Assert.False(b.Attack(b.Champions[1], null, 1.0)[0].Spark);
    }

    [Fact]
    public void OwnMultiHitDoesNotSparkItself()
    {
        var b = NewBattle(Stage(Wave(Enemy(hp: 5000))), Champion(hits: [0.5f, 0.5f]));
        var hits = b.Attack(b.Champions[0], null, 0);
        Assert.Equal(2, hits.Count);
        Assert.All(hits, h => Assert.False(h.Spark));
    }

    [Fact]
    public void HitsFillTheGauge()
    {
        var emp = new GameModel.Units.EmpowerDef { Name = "Tidal Blade" };
        var b = NewBattle(Stage(Wave(Enemy(hp: 5000))), Champion(gaugeMax: 20, empower: emp));
        var knight = b.Champions[0];
        b.Attack(knight, null, 0);
        Assert.InRange(knight.Gauge, 1, 2);
    }

    [Fact]
    public void ClearingAWaveSpawnsTheNext()
    {
        var b = NewBattle(Stage(Wave(Enemy(hp: 10)), Wave(Enemy("Orc", hp: 10))), Champion());
        b.Attack(b.Champions[0], null, 0);
        Assert.Equal(1, b.WaveIndex);
        Assert.Equal("Orc", b.Enemies[0].Name);
        Assert.Equal(BattlePhase.Fighting, b.Phase);
    }

    [Fact]
    public void ClearingTheLastWaveWins()
    {
        var b = NewBattle(Stage(Wave(Enemy(hp: 10))), Champion());
        b.Attack(b.Champions[0], null, 0);
        Assert.Equal(BattlePhase.Won, b.Phase);
        Assert.Empty(b.Attack(b.Champions[0], null, 10));
    }
}
