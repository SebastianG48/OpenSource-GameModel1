using System.Text.Json;
using System.Text.Json.Serialization;
using GameModel.Battle;
using GameModel.Units;

namespace GameModel.Data;

/// <summary>A stage file: the stage plus the squad (by name) that fights it.</summary>
public sealed record StageFile(StageDef Stage, IReadOnlyList<string> Squad);

/// <summary>
/// Reads champions and stages from JSON, so designers can add content and retune stats without
/// touching code. Bad data fails loudly here, at load time, instead of halfway through a battle.
/// </summary>
public static class DataLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private sealed record ChampionList(List<UnitDef> Champions);

    private sealed record WaveJson(List<UnitDef> Enemies);

    private sealed record StageJson(string Name, List<string>? Squad, List<WaveJson> Waves);

    public static IReadOnlyDictionary<string, UnitDef> ParseChampions(string json)
    {
        var list = JsonSerializer.Deserialize<ChampionList>(json, Options)?.Champions
                   ?? throw new InvalidDataException("No \"champions\" array.");
        var byName = new Dictionary<string, UnitDef>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in list)
        {
            Validate(c);
            if (!byName.TryAdd(c.Name, c)) throw new InvalidDataException($"Duplicate champion \"{c.Name}\".");
        }
        return byName;
    }

    public static StageFile ParseStage(string json)
    {
        var s = JsonSerializer.Deserialize<StageJson>(json, Options)
                ?? throw new InvalidDataException("Empty stage file.");
        if (s.Waves is not { Count: > 0 }) throw new InvalidDataException($"Stage \"{s.Name}\" has no waves.");
        foreach (var e in s.Waves.SelectMany(w => w.Enemies)) Validate(e);

        var stage = new StageDef { Name = s.Name, Waves = s.Waves.Select(w => new WaveDef(w.Enemies)).ToList() };
        return new StageFile(stage, s.Squad ?? []);
    }

    /// <summary>Looks up the stage's squad by name.</summary>
    public static List<UnitDef> ResolveSquad(StageFile stage, IReadOnlyDictionary<string, UnitDef> champions) =>
        stage.Squad.Select(n => champions.TryGetValue(n, out var c)
            ? c
            : throw new InvalidDataException($"Unknown champion \"{n}\" in squad.")).ToList();

    private static void Validate(UnitDef u)
    {
        if (string.IsNullOrWhiteSpace(u.Name)) throw new InvalidDataException("A unit has no name.");
        if (u.Hp <= 0 || u.Atk < 0 || u.Def < 0) throw new InvalidDataException($"{u.Name}: stats must be positive.");
        if (u.HitWeights.Length == 0) throw new InvalidDataException($"{u.Name}: needs at least one hit.");
        if (u.Empower != null && u.GaugeMax <= 0)
            throw new InvalidDataException($"{u.Name}: has an Empower but no gauge.");
    }
}
