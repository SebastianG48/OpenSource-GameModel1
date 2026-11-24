namespace GameModel.Core;

/// <summary>
/// Source of randomness for the battle. Battles take one of these instead of calling
/// <see cref="Random.Shared"/>, so a test or a replay can use a fixed seed.
/// </summary>
public interface IRandom
{
    /// <summary>A number in [0, 1).</summary>
    double NextDouble();

    /// <summary>A whole number in [min, max] inclusive.</summary>
    int Range(int min, int max);
}

/// <summary>Seeded <see cref="IRandom"/> built on <see cref="Random"/>. Same seed, same battle.</summary>
public sealed class SeededRandom(int seed) : IRandom
{
    private readonly Random _random = new(seed);

    public double NextDouble() => _random.NextDouble();

    public int Range(int min, int max) => _random.Next(min, max + 1);

    /// <summary>A number in [min, max).</summary>
    public double Range(double min, double max) => min + NextDouble() * (max - min);
}

/// <summary>Always returns the same value. Handy for tests that must not roll dice.</summary>
public sealed class FixedRandom(double value) : IRandom
{
    public double NextDouble() => value;

    public int Range(int min, int max) => min + (int)Math.Floor(value * (max - min + 1));
}
