using GameModel.Battle;
using GameModel.Core;
using GameModel.Elements;
using GameModel.Units;
using static GameModel.Tests.TestData;

namespace GameModel.Tests;

public class FreezeTests
{
    private static readonly EmpowerDef FrostLance = new()
    {
        Name = "Frost Lance", Power = 1f, FreezeChance = 1f, FreezeDuration = 3,
    };

    private static GameModel.Battle.Battle Start()
    {
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy(hp: 5000, element: Element.Light))),
            [Champion("Ice Mage", gaugeMax: 10, empower: FrostLance), Champion("Knight")],
            new FixedRandom(0.5));
        b.Start();
        b.Champions[0].AddGauge(10);
        return b;
    }

    [Fact]
    public void FreezeEmpowerFreezesTheTarget()
    {
        var b = Start();
        var hit = b.UseEmpower(b.Champions[0], null, 0).Single();
        Assert.True(hit.Froze);
        Assert.True(b.Enemies[0].IsFrozen(1.0));
        Assert.False(b.Enemies[0].IsFrozen(3.0));
    }

    [Fact]
    public void FrozenEnemyCannotAct()
    {
        var b = Start();
        b.UseEmpower(b.Champions[0], null, 0);
        Assert.Empty(b.Tick(2.9));
    }

    [Fact]
    public void FreezePushesBackTheEnemyTurn()
    {
        var b = Start();
        double before = b.Enemies[0].NextActionAt;
        b.UseEmpower(b.Champions[0], null, 0);
        Assert.Equal(before + 3, b.Enemies[0].NextActionAt, 3);
    }

    [Fact]
    public void NextHitShattersForBonusDamage()
    {
        var b = Start();
        b.UseEmpower(b.Champions[0], null, 0);
        var hit = b.Attack(b.Champions[1], null, 1.0).Single();
        Assert.True(hit.Shattered);
        Assert.Equal(150, hit.Damage);
        Assert.False(b.Enemies[0].IsFrozen(1.5));
    }

    [Fact]
    public void ZeroChanceNeverFreezes()
    {
        var emp = FrostLance with { FreezeChance = 0 };
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy(hp: 5000))), [Champion(gaugeMax: 10, empower: emp)],
            new FixedRandom(0.5));
        b.Start();
        b.Champions[0].AddGauge(10);
        Assert.False(b.UseEmpower(b.Champions[0], null, 0).Single().Froze);
    }
}
