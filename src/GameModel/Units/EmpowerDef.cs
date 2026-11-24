namespace GameModel.Units;

/// <summary>Who an Empower hits or helps.</summary>
public enum EmpowerTarget
{
    SingleEnemy,
    AllEnemies,
    AllAllies,
}

/// <summary>
/// A champion's special move, unlocked when its gauge is full. It can deal damage, heal the squad,
/// freeze enemies, or a mix.
/// </summary>
public sealed record EmpowerDef
{
    public required string Name { get; init; }

    /// <summary>Damage power, as a multiple of ATK. 0 for a pure heal.</summary>
    public float Power { get; init; } = 2f;

    /// <summary>How the power splits across hits.</summary>
    public float[] HitWeights { get; init; } = [1f];

    public EmpowerTarget Target { get; init; } = EmpowerTarget.SingleEnemy;

    /// <summary>For an ally Empower: share of each champion's max HP restored, plus REC.</summary>
    public float HealFraction { get; init; }

    /// <summary>Chance (0..1) that each enemy hit is frozen.</summary>
    public float FreezeChance { get; init; }

    /// <summary>Seconds a freeze lasts.</summary>
    public double FreezeDuration { get; init; } = 3;
}
