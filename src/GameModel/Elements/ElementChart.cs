namespace GameModel.Elements;

/// <summary>
/// Which element beats which, and how much that changes damage.
/// The default chart is a five-element cycle (Fire, Grass, Earth, Lightning, Water)
/// plus Light and Dark, which beat each other.
/// </summary>
public sealed class ElementChart
{
    private readonly Dictionary<Element, HashSet<Element>> _beats = new();

    /// <summary>Damage multiplier when the attacker has the advantage.</summary>
    public float StrongMultiplier { get; init; } = 1.5f;

    /// <summary>Damage multiplier when the defender has the advantage.</summary>
    public float WeakMultiplier { get; init; } = 0.75f;

    /// <summary>Records that <paramref name="attacker"/> deals bonus damage to each of <paramref name="victims"/>.</summary>
    public ElementChart SetBeats(Element attacker, params Element[] victims)
    {
        if (!_beats.TryGetValue(attacker, out var set))
        {
            set = new HashSet<Element>();
            _beats[attacker] = set;
        }

        foreach (var v in victims)
        {
            if (v == attacker)
                throw new ArgumentException($"{attacker} cannot beat itself.", nameof(victims));
            set.Add(v);
        }

        return this;
    }

    /// <summary>True when <paramref name="attacker"/> is strong against <paramref name="defender"/>.</summary>
    public bool Beats(Element attacker, Element defender) =>
        _beats.TryGetValue(attacker, out var set) && set.Contains(defender);

    /// <summary>Damage multiplier for an attack of <paramref name="attack"/> against a unit of <paramref name="defender"/>.</summary>
    public float Multiplier(Element attack, Element defender)
    {
        if (Beats(attack, defender)) return StrongMultiplier;
        if (Beats(defender, attack)) return WeakMultiplier;
        return 1f;
    }

    /// <summary>The chart the game uses.</summary>
    public static ElementChart CreateDefault() => new ElementChart()
        .SetBeats(Element.Fire, Element.Grass)
        .SetBeats(Element.Grass, Element.Earth)
        .SetBeats(Element.Earth, Element.Lightning)
        .SetBeats(Element.Lightning, Element.Water)
        .SetBeats(Element.Water, Element.Fire)
        .SetBeats(Element.Light, Element.Dark)
        .SetBeats(Element.Dark, Element.Light);
}
