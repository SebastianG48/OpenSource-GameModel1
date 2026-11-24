using GameModel.Core;

namespace GameModel.Tests;

public class RandomTests
{
    [Fact]
    public void SameSeedGivesSameSequence()
    {
        var a = new SeededRandom(42);
        var b = new SeededRandom(42);
        for (int i = 0; i < 20; i++) Assert.Equal(a.NextDouble(), b.NextDouble());
    }

    [Fact]
    public void RangeIsInclusive()
    {
        var r = new SeededRandom(7);
        var seen = new HashSet<int>();
        for (int i = 0; i < 200; i++) seen.Add(r.Range(1, 2));
        Assert.Equal(new HashSet<int> { 1, 2 }, seen);
    }

    [Fact]
    public void FixedRandomNeverChanges()
    {
        var r = new FixedRandom(0.5);
        Assert.Equal(0.5, r.NextDouble());
        Assert.Equal(0.5, r.NextDouble());
    }
}
