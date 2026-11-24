using GameModel.Battle;
using GameModel.Core;
using GameModel.Elements;
using GameModel.Units;
using static GameModel.Tests.TestData;

namespace GameModel.Tests;

public class EmpowerTests
{
    private static GameModel.Battle.Battle Start(StageDef stage, params UnitDef[] squad)
    {
        var b = new GameModel.Battle.Battle(stage, squad, new FixedRandom(0.5));
        b.Start();
        return b;
    }

    [Fact]
    public void EmpowerNeedsAFullGauge()
    {
        var emp = new EmpowerDef { Name = "Tidal Blade" };
        var b = Start(Stage(Wave(Enemy(hp: 5000))), Champion(gaugeMax: 10, empower: emp));
        var knight = b.Champions[0];
        Assert.False(b.CanEmpower(knight, 0));
        knight.AddGauge(10);
        Assert.True(b.CanEmpower(knight, 0));
    }

    [Fact]
    public void EmpowerHitsHarderAndEmptiesGauge()
    {
        var emp = new EmpowerDef { Name = "Tidal Blade", Power = 3f };
        var b = Start(Stage(Wave(Enemy(hp: 5000, element: Element.Light))), Champion(gaugeMax: 10, empower: emp));
        var knight = b.Champions[0];
        knight.AddGauge(10);
        var hits = b.UseEmpower(knight, null, 0);
        Assert.Equal(300, hits[0].Damage);
        Assert.True(knight.Gauge < 10);
    }

    [Fact]
    public void EmpowerIgnoresAttackCooldown()
    {
        var emp = new EmpowerDef { Name = "Tidal Blade" };
        var b = Start(Stage(Wave(Enemy(hp: 5000))), Champion(gaugeMax: 10, empower: emp));
        var knight = b.Champions[0];
        b.Attack(knight, null, 0);
        knight.AddGauge(10);
        Assert.NotEmpty(b.UseEmpower(knight, null, 0.5));
    }

    [Fact]
    public void AreaEmpowerHitsEveryEnemy()
    {
        var emp = new EmpowerDef { Name = "Firestorm", Target = EmpowerTarget.AllEnemies, Power = 1f };
        var b = Start(Stage(Wave(Enemy("A", hp: 5000), Enemy("B", hp: 5000), Enemy("C", hp: 5000))),
            Champion(gaugeMax: 10, empower: emp));
        b.Champions[0].AddGauge(10);
        var hits = b.UseEmpower(b.Champions[0], null, 0);
        Assert.Equal(3, hits.Select(h => h.Target).Distinct().Count());
    }

    [Fact]
    public void HealingEmpowerRestoresTheSquad()
    {
        var emp = new EmpowerDef { Name = "Dawn Prayer", Target = EmpowerTarget.AllAllies, Power = 0, HealFraction = 0.2f };
        var b = Start(Stage(Wave(Enemy())), Champion("Priest", gaugeMax: 10, empower: emp), Champion("Knight"));
        var knight = b.Champions[1];
        knight.TakeDamage(500);
        b.Champions[0].AddGauge(10);
        var heals = b.UseEmpower(b.Champions[0], null, 0);
        // 20% of 1000 + REC 50
        Assert.Equal(250, heals.Single(h => h.Target == knight).Healed);
        Assert.Equal(750, knight.Hp);
    }
}
