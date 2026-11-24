using GameModel.Battle;

namespace GameModel.Tests;

public class DamageCalculatorTests
{
    private static readonly BattleTuning T = BattleTuning.Default;

    [Fact]
    public void BasicHitSubtractsHalfDefense()
    {
        // 100 ATK - 40 DEF * 0.5 = 80
        Assert.Equal(80, DamageCalculator.Compute(100, 40, 1f, 1f, false, 0, T));
    }

    [Fact]
    public void ElementAdvantageScalesDamage()
    {
        Assert.Equal(120, DamageCalculator.Compute(100, 40, 1f, 1.5f, false, 0, T));
        Assert.Equal(60, DamageCalculator.Compute(100, 40, 1f, 0.75f, false, 0, T));
    }

    [Fact]
    public void SparkMultipliesDamage()
    {
        Assert.Equal(120, DamageCalculator.Compute(100, 40, 1f, 1f, true, 0, T));
    }

    [Fact]
    public void HighDefenseStillLetsMinimumThrough()
    {
        // raw 100, mitigation 500 -> floor at 15% of raw
        Assert.Equal(15, DamageCalculator.Compute(100, 1000, 1f, 1f, false, 0, T));
    }

    [Fact]
    public void VarianceStaysInsideItsBand()
    {
        int low = DamageCalculator.Compute(1000, 0, 1f, 1f, false, -1, T);
        int high = DamageCalculator.Compute(1000, 0, 1f, 1f, false, 1, T);
        Assert.Equal(920, low);
        Assert.Equal(1080, high);
    }

    [Fact]
    public void EmpowerPowerDoesNotScaleDefense()
    {
        // power 3: raw 300, mitigation 40 * 0.5 * 1 = 20
        Assert.Equal(280, DamageCalculator.Compute(100, 40, 3f, 1f, false, 0, T));
    }

    [Fact]
    public void AnyHitDealsAtLeastOne()
    {
        Assert.Equal(1, DamageCalculator.Compute(1, 999, 0.1f, 0.5f, false, -1, T));
    }

    [Fact]
    public void ZeroPowerDealsNothing()
    {
        Assert.Equal(0, DamageCalculator.Compute(100, 0, 0f, 1f, false, 0, T));
    }
}
