using GameModel.Core;
using GameModel.Elements;
using static GameModel.Tests.TestData;

namespace GameModel.Tests;

public class BossTests
{
    private static GameModel.Battle.Battle BossFight(int bossHp)
    {
        var boss = Enemy("Cyclops", Element.Light, hp: bossHp, atk: 100, boss: true);
        var b = new GameModel.Battle.Battle(Stage(Wave(boss)), [Champion(atk: 100)], new FixedRandom(0.5));
        b.Start();
        return b;
    }

    [Fact]
    public void BossEnragesBelowHalfHp()
    {
        var b = BossFight(150);
        var hit = b.Attack(b.Champions[0], null, 0).Single();
        Assert.True(hit.Enraged);
        Assert.Equal(130, b.Enemies[0].Atk);
    }

    [Fact]
    public void BossEnragesOnlyOnce()
    {
        var b = BossFight(300);
        b.Attack(b.Champions[0], null, 0);
        b.Attack(b.Champions[0], null, 10);
        Assert.True(b.Enemies[0].Enraged);
        Assert.False(b.Attack(b.Champions[0], null, 20).Single().Enraged);
        Assert.Equal(130, b.Enemies[0].Atk);
    }

    [Fact]
    public void NormalEnemiesNeverEnrage()
    {
        var b = new GameModel.Battle.Battle(Stage(Wave(Enemy(hp: 150, element: Element.Light))), [Champion()], new FixedRandom(0.5));
        b.Start();
        Assert.False(b.Attack(b.Champions[0], null, 0).Single().Enraged);
        Assert.False(b.Enemies[0].Enraged);
    }

    [Fact]
    public void BossActsOnItsOwnSlowerTimer()
    {
        var b = BossFight(5000);
        Assert.Empty(b.Tick(5.5));
        Assert.NotEmpty(b.Tick(6.0));
    }
}
