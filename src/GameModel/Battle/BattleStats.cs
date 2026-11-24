namespace GameModel.Battle;

/// <summary>Running totals for the results screen and balance checks.</summary>
public sealed class BattleStats
{
    public int Sparks { get; internal set; }
    public int Empowers { get; internal set; }
    public int Kills { get; internal set; }
    public int BestHit { get; internal set; }
    public int DamageDealt { get; internal set; }
    public int DamageTaken { get; internal set; }
}
