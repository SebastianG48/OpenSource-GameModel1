using GameModel.Units;

namespace GameModel.Battle;

/// <summary>What one hit did. A view layer can turn these into numbers, flashes and sounds.</summary>
public sealed record HitResult
{
    public required Unit Attacker { get; init; }
    public required Unit Target { get; init; }
    public required double Time { get; init; }
    public int Damage { get; init; }
    public int Healed { get; init; }
    public bool Spark { get; init; }
    public float ElementMultiplier { get; init; } = 1f;
    public bool Killed { get; init; }
    public int Crystals { get; init; }

    /// <summary>This hit froze the target.</summary>
    public bool Froze { get; init; }

    /// <summary>This hit broke an existing freeze for bonus damage.</summary>
    public bool Shattered { get; init; }

    /// <summary>This hit pushed a boss into its enraged phase.</summary>
    public bool Enraged { get; init; }
}
