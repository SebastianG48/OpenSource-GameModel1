using GameModel.Units;

namespace GameModel.Battle;

/// <summary>One group of enemies. A stage's waves are fought one after another.</summary>
public sealed record WaveDef(IReadOnlyList<UnitDef> Enemies);

/// <summary>A playable stage: a name and its waves. The last wave usually holds the boss.</summary>
public sealed record StageDef
{
    public required string Name { get; init; }
    public required IReadOnlyList<WaveDef> Waves { get; init; }
}
