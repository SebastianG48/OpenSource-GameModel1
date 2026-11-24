using GameModel.Elements;

namespace GameModel.Units;

/// <summary>Which team a unit fights for.</summary>
public enum Side
{
    Champion,
    Enemy,
}

/// <summary>
/// The fixed data for a unit: what it is before a battle starts. A <see cref="Unit"/> is the live copy
/// that takes damage during a fight.
/// </summary>
public sealed record UnitDef
{
    public required string Name { get; init; }
    public required Element Element { get; init; }
    public required int Hp { get; init; }
    public required int Atk { get; init; }
    public required int Def { get; init; }

    /// <summary>Recovery: how strongly this unit heals.</summary>
    public int Rec { get; init; }

    /// <summary>
    /// How a normal attack's damage splits across its hits. {1} is one hit; {0.25, 0.25, 0.5} is three hits.
    /// </summary>
    public float[] HitWeights { get; init; } = [1f];

    /// <summary>Gauge needed before this unit can use its Empower. 0 means it has none.</summary>
    public int GaugeMax { get; init; }

    /// <summary>The special move this unit uses when its gauge is full. Enemies usually have none.</summary>
    public EmpowerDef? Empower { get; init; }

    public bool IsBoss { get; init; }
}
