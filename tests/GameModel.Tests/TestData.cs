using GameModel.Battle;
using GameModel.Elements;
using GameModel.Units;

namespace GameModel.Tests;

/// <summary>Small units and stages shared by the battle tests.</summary>
internal static class TestData
{
    public static UnitDef Champion(string name = "Knight", Element element = Element.Water, int atk = 100,
        int hp = 1000, float[]? hits = null, int gaugeMax = 0, EmpowerDef? empower = null) => new()
    {
        Name = name, Element = element, Hp = hp, Atk = atk, Def = 20, Rec = 50,
        HitWeights = hits ?? [1f], GaugeMax = gaugeMax, Empower = empower,
    };

    public static UnitDef Enemy(string name = "Goblin", Element element = Element.Fire, int hp = 500,
        int atk = 60, bool boss = false) => new()
    {
        Name = name, Element = element, Hp = hp, Atk = atk, Def = 0, IsBoss = boss,
    };

    public static StageDef Stage(params WaveDef[] waves) => new() { Name = "Test", Waves = waves };

    public static WaveDef Wave(params UnitDef[] enemies) => new(enemies);
}
