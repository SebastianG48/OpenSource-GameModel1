using GameModel.Elements;

namespace GameModel.Units;

/// <summary>A unit inside a battle. Holds its current HP and per-battle state.</summary>
public sealed class Unit
{
    public Unit(int id, UnitDef def, Side side)
    {
        if (def.Hp <= 0) throw new ArgumentOutOfRangeException(nameof(def), "A unit needs positive HP.");
        Id = id;
        Def = def;
        Side = side;
        MaxHp = def.Hp;
        Hp = def.Hp;
        Atk = def.Atk;
        Defense = def.Def;
    }

    public int Id { get; }
    public UnitDef Def { get; }
    public Side Side { get; }
    public string Name => Def.Name;
    public Element Element => Def.Element;
    public bool IsBoss => Def.IsBoss;

    public int MaxHp { get; }
    public int Hp { get; private set; }
    public int Atk { get; set; }
    public int Defense { get; set; }

    public bool Alive => Hp > 0;
    public float HpFraction => (float)Hp / MaxHp;

    /// <summary>Takes damage off HP, never below zero. Returns the damage actually taken.</summary>
    public int TakeDamage(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        int before = Hp;
        Hp = Math.Max(0, Hp - amount);
        return before - Hp;
    }

    /// <summary>Restores HP, never above max. A fallen unit cannot be healed. Returns the HP gained.</summary>
    public int Heal(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (!Alive) return 0;
        int before = Hp;
        Hp = Math.Min(MaxHp, Hp + amount);
        return Hp - before;
    }

    public override string ToString() => $"{Name} ({Element}) {Hp}/{MaxHp}";
}
