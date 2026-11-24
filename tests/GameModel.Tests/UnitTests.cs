using GameModel.Elements;
using GameModel.Units;

namespace GameModel.Tests;

public class UnitTests
{
    private static Unit Make(int hp = 100) =>
        new(1, new UnitDef { Name = "Knight", Element = Element.Water, Hp = hp, Atk = 50, Def = 20 }, Side.Champion);

    [Fact]
    public void DamageStopsAtZero()
    {
        var u = Make();
        Assert.Equal(100, u.TakeDamage(250));
        Assert.Equal(0, u.Hp);
        Assert.False(u.Alive);
    }

    [Fact]
    public void HealingStopsAtMax()
    {
        var u = Make();
        u.TakeDamage(30);
        Assert.Equal(30, u.Heal(80));
        Assert.Equal(100, u.Hp);
    }

    [Fact]
    public void FallenUnitsCannotBeHealed()
    {
        var u = Make();
        u.TakeDamage(100);
        Assert.Equal(0, u.Heal(50));
        Assert.False(u.Alive);
    }

    [Fact]
    public void UnitNeedsPositiveHp()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Make(0));
    }
}
