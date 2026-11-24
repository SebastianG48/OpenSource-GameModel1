using GameModel.Elements;

namespace GameModel.Tests;

public class ElementChartTests
{
    private readonly ElementChart _chart = ElementChart.CreateDefault();

    [Theory]
    [InlineData(Element.Fire, Element.Grass)]
    [InlineData(Element.Grass, Element.Earth)]
    [InlineData(Element.Earth, Element.Lightning)]
    [InlineData(Element.Lightning, Element.Water)]
    [InlineData(Element.Water, Element.Fire)]
    [InlineData(Element.Light, Element.Dark)]
    [InlineData(Element.Dark, Element.Light)]
    public void StrongMatchupsUseStrongMultiplier(Element attack, Element defend)
    {
        Assert.Equal(1.5f, _chart.Multiplier(attack, defend));
    }

    [Fact]
    public void ReverseMatchupIsWeak()
    {
        Assert.Equal(0.75f, _chart.Multiplier(Element.Grass, Element.Fire));
    }

    [Fact]
    public void UnrelatedElementsAreNeutral()
    {
        Assert.Equal(1f, _chart.Multiplier(Element.Fire, Element.Light));
        Assert.Equal(1f, _chart.Multiplier(Element.Water, Element.Water));
    }

    [Fact]
    public void EveryCycleElementBeatsExactlyOneOther()
    {
        foreach (var a in Enum.GetValues<Element>())
        {
            int wins = Enum.GetValues<Element>().Count(d => _chart.Beats(a, d));
            Assert.Equal(1, wins);
        }
    }

    [Fact]
    public void AnElementCannotBeatItself()
    {
        Assert.Throws<ArgumentException>(() => new ElementChart().SetBeats(Element.Fire, Element.Fire));
    }
}
