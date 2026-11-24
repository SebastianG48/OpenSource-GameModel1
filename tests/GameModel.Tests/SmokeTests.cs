namespace GameModel.Tests;

public class SmokeTests
{
    [Fact]
    public void LibraryAssemblyLoads()
    {
        Assert.NotNull(typeof(GameModel.AssemblyMarker).Assembly);
    }
}
