namespace GameModel.Battle;

/// <summary>
/// Every number that shapes how a battle feels, in one place so balance can change without code changes.
/// Times are in seconds of battle time. The defaults are the values the game ships with.
/// </summary>
public sealed record BattleTuning
{
    /// <summary>Two hits from different champions within this many seconds make a Spark.</summary>
    public double SparkWindow { get; init; } = 0.28;

    /// <summary>Damage multiplier on a Spark hit.</summary>
    public float SparkMultiplier { get; init; } = 1.5f;

    /// <summary>How much of the defender's DEF is subtracted from raw damage.</summary>
    public float DefenseFactor { get; init; } = 0.5f;

    /// <summary>A hit always deals at least this share of its raw damage, however high DEF is.</summary>
    public float MinimumDamageShare { get; init; } = 0.15f;

    /// <summary>Random spread on damage: 0.08 means plus or minus 8 percent.</summary>
    public float DamageVariance { get; init; } = 0.08f;

    /// <summary>Seconds between the hits of a multi-hit attack.</summary>
    public double HitSpacing { get; init; } = 0.11;

    /// <summary>Gauge crystals gained per hit, rolled between these two values.</summary>
    public int CrystalsPerHitMin { get; init; } = 1;
    public int CrystalsPerHitMax { get; init; } = 2;
    public int CrystalsOnSpark { get; init; } = 1;
    public int CrystalsOnKill { get; init; } = 4;

    /// <summary>Empower hits give only this share of the normal crystals, so Empowers cannot chain forever.</summary>
    public float EmpowerCrystalMultiplier { get; init; } = 0.25f;

    /// <summary>Seconds a champion waits after a normal attack before it can attack again.</summary>
    public double ChampionCooldown { get; init; } = 3.5;

    /// <summary>Seconds between enemy attacks, rolled in this range.</summary>
    public double EnemyIntervalMin { get; init; } = 4.0;
    public double EnemyIntervalMax { get; init; } = 5.0;

    /// <summary>Seconds between boss attacks.</summary>
    public double BossInterval { get; init; } = 6.0;

    /// <summary>A boss under this share of HP enrages once.</summary>
    public float EnrageThreshold { get; init; } = 0.5f;

    /// <summary>ATK multiplier when a boss enrages.</summary>
    public float EnrageAtkMultiplier { get; init; } = 1.3f;

    /// <summary>Damage multiplier on the hit that shatters a frozen enemy.</summary>
    public float ShatterMultiplier { get; init; } = 1.5f;

    public static BattleTuning Default { get; } = new();
}
