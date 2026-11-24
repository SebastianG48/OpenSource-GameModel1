using GameModel.Data;
using GameModel.Elements;
using GameModel.Units;

namespace GameModel.Tests;

public class DataLoaderTests
{
    private static string DataFile(string name)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "data"))) dir = dir.Parent;
        return File.ReadAllText(Path.Combine(dir!.FullName, "data", name));
    }

    [Fact]
    public void ShippedChampionsLoad()
    {
        var champs = DataLoader.ParseChampions(DataFile("champions.json"));
        Assert.True(champs.Count >= 5);
        var mage = champs["Frost Mage"];
        Assert.Equal(Element.Water, mage.Element);
        Assert.Equal(EmpowerTarget.AllEnemies, mage.Empower!.Target);
        Assert.Equal(0.6f, mage.Empower.FreezeChance);
    }

    [Fact]
    public void ShippedStageLoadsWithItsSquad()
    {
        var champs = DataLoader.ParseChampions(DataFile("champions.json"));
        var stage = DataLoader.ParseStage(DataFile("stage-forest-road.json"));
        Assert.Equal(3, stage.Stage.Waves.Count);
        Assert.True(stage.Stage.Waves[^1].Enemies.Single().IsBoss);
        Assert.Equal(5, DataLoader.ResolveSquad(stage, champs).Count);
    }

    [Fact]
    public void DuplicateNamesAreRejected()
    {
        const string json = """
            { "champions": [
              { "name": "A", "element": "Fire", "hp": 1, "atk": 1, "def": 1 },
              { "name": "a", "element": "Fire", "hp": 1, "atk": 1, "def": 1 } ] }
            """;
        Assert.Throws<InvalidDataException>(() => DataLoader.ParseChampions(json));
    }

    [Fact]
    public void EmpowerWithoutGaugeIsRejected()
    {
        const string json = """
            { "champions": [ { "name": "A", "element": "Fire", "hp": 10, "atk": 1, "def": 1,
              "empower": { "name": "Boom" } } ] }
            """;
        Assert.Throws<InvalidDataException>(() => DataLoader.ParseChampions(json));
    }

    [Fact]
    public void UnknownSquadMemberIsRejected()
    {
        var stage = DataLoader.ParseStage("""
            { "name": "S", "squad": ["Nobody"], "waves": [ { "enemies": [
              { "name": "E", "element": "Dark", "hp": 5, "atk": 1, "def": 0 } ] } ] }
            """);
        Assert.Throws<InvalidDataException>(() => DataLoader.ResolveSquad(stage, new Dictionary<string, UnitDef>()));
    }
}
