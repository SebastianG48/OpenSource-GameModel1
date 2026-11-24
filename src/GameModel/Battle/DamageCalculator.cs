namespace GameModel.Battle;

/// <summary>
/// The damage formula, kept pure (no state, no dice) so it is easy to test.
/// <code>
/// raw     = ATK * power
/// reduced = max(raw * minimumShare, raw - DEF * defenseFactor * min(1, power))
/// damage  = reduced * element * (1 + varianceRoll * variance) * (spark ? sparkMult : 1)
/// </code>
/// DEF is scaled by min(1, power) so a weak hit (power below 1) is not wiped out by armour, while
/// a strong Empower (power above 1) does not have to punch through more armour than a normal attack.
/// </summary>
public static class DamageCalculator
{
    /// <param name="atk">Attacker ATK.</param>
    /// <param name="def">Defender DEF.</param>
    /// <param name="power">Share of ATK this hit uses (a hit weight times any Empower power).</param>
    /// <param name="elementMultiplier">From the element chart.</param>
    /// <param name="spark">True when this hit landed inside the Spark window.</param>
    /// <param name="varianceRoll">A roll in [-1, 1]; 0 gives the average.</param>
    /// <param name="tuning">Balance values.</param>
    /// <returns>Damage, at least 1.</returns>
    public static int Compute(int atk, int def, float power, float elementMultiplier, bool spark,
        double varianceRoll, BattleTuning tuning)
    {
        if (power <= 0) return 0;
        varianceRoll = Math.Clamp(varianceRoll, -1, 1);

        float raw = atk * power;
        float mitigation = def * tuning.DefenseFactor * Math.Min(1f, power);
        float reduced = Math.Max(raw * tuning.MinimumDamageShare, raw - mitigation);

        float damage = reduced * elementMultiplier * (1f + (float)varianceRoll * tuning.DamageVariance);
        if (spark) damage *= tuning.SparkMultiplier;

        return Math.Max(1, (int)Math.Round(damage));
    }
}
