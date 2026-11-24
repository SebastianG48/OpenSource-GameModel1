using GameModel.Core;
using GameModel.Elements;
using GameModel.Units;

namespace GameModel.Battle;

public enum BattlePhase
{
    NotStarted,
    Fighting,
    Won,
    Lost,
}

/// <summary>
/// The rules of one battle, with no graphics or input. The caller passes the current battle time
/// into every method, so the same battle plays the same way in the game, in a test, or in a
/// fast-forward simulation.
/// </summary>
public sealed partial class Battle
{
    private readonly StageDef _stage;
    private readonly IRandom _rng;
    private readonly List<Unit> _champions = new();
    private readonly List<Unit> _enemies = new();
    private readonly List<string> _log = new();
    private int _nextId = 1;

    public Battle(StageDef stage, IReadOnlyList<UnitDef> squad, IRandom rng,
        ElementChart? chart = null, BattleTuning? tuning = null)
    {
        if (stage.Waves.Count == 0) throw new ArgumentException("A stage needs at least one wave.", nameof(stage));
        if (squad.Count == 0) throw new ArgumentException("A squad needs at least one champion.", nameof(squad));

        _stage = stage;
        _rng = rng;
        Chart = chart ?? ElementChart.CreateDefault();
        Tuning = tuning ?? BattleTuning.Default;

        foreach (var def in squad) _champions.Add(new Unit(_nextId++, def, Side.Champion));
    }

    public ElementChart Chart { get; }
    public BattleTuning Tuning { get; }
    public BattlePhase Phase { get; private set; } = BattlePhase.NotStarted;
    public bool Fighting => Phase == BattlePhase.Fighting;

    /// <summary>Zero-based index of the current wave.</summary>
    public int WaveIndex { get; private set; } = -1;
    public int WaveCount => _stage.Waves.Count;

    public IReadOnlyList<Unit> Champions => _champions;
    public IReadOnlyList<Unit> Enemies => _enemies;
    public IEnumerable<Unit> AliveChampions => _champions.Where(c => c.Alive);
    public IEnumerable<Unit> AliveEnemies => _enemies.Where(e => e.Alive);

    /// <summary>A plain-text record of what happened, for the console demo and debugging.</summary>
    public IReadOnlyList<string> Log => _log;

    public void Start(double now = 0)
    {
        if (Phase != BattlePhase.NotStarted) throw new InvalidOperationException("Battle already started.");
        Phase = BattlePhase.Fighting;
        foreach (var c in _champions) c.NextActionAt = now;
        SpawnWave(0, now);
    }

    // ------------------------------------------------------------------ champion attacks

    public bool CanAttack(Unit champion, double now) =>
        Fighting && champion.Side == Side.Champion && champion.Alive && now >= champion.NextActionAt;

    /// <summary>
    /// The enemy a champion should hit: the one its element beats, then the one closest to falling.
    /// </summary>
    public Unit? DefaultTarget(Unit attacker) => AliveEnemies
        .OrderByDescending(e => Chart.Multiplier(attacker.Element, e.Element))
        .ThenBy(e => e.Hp)
        .FirstOrDefault();

    /// <summary>A normal attack. Each hit lands <see cref="BattleTuning.HitSpacing"/> seconds after the last.</summary>
    public IReadOnlyList<HitResult> Attack(Unit champion, Unit? target, double now)
    {
        if (!CanAttack(champion, now)) return [];
        target = target is { Alive: true } ? target : DefaultTarget(champion);
        if (target == null) return [];

        var results = new List<HitResult>();
        var weights = champion.Def.HitWeights;
        for (int i = 0; i < weights.Length && target.Alive; i++)
            results.Add(ResolveHit(champion, target, weights[i], now + i * Tuning.HitSpacing, empower: false));

        champion.NextActionAt = now + Tuning.ChampionCooldown;
        AfterAction(now);
        return results;
    }

    // ------------------------------------------------------------------ hits

    private HitResult ResolveHit(Unit actor, Unit target, float power, double time, bool empower)
    {
        bool spark = time - target.LastHitTime <= Tuning.SparkWindow
                     && target.LastHitById != actor.Id
                     && target.LastHitById != -1;
        float elem = Chart.Multiplier(actor.Element, target.Element);
        double roll = _rng.NextDouble() * 2 - 1;

        int damage = DamageCalculator.Compute(actor.Atk, target.Defense, power, elem, spark, roll, Tuning);
        target.TakeDamage(damage);
        target.LastHitTime = time;
        target.LastHitById = actor.Id;
        bool killed = !target.Alive;

        int crystals = 0;
        if (actor.Side == Side.Champion)
        {
            crystals = _rng.Range(Tuning.CrystalsPerHitMin, Tuning.CrystalsPerHitMax);
            if (spark) crystals += Tuning.CrystalsOnSpark;
            if (killed) crystals += Tuning.CrystalsOnKill;
            float gain = empower ? crystals * Tuning.EmpowerCrystalMultiplier : crystals;
            actor.AddGauge(gain);
        }

        if (killed) Write(time, $"{target.Name} falls to {actor.Name}.");

        return new HitResult
        {
            Attacker = actor, Target = target, Time = time, Damage = damage, Spark = spark,
            ElementMultiplier = elem, Killed = killed, Crystals = crystals,
        };
    }

    // ------------------------------------------------------------------ waves and outcome

    private void SpawnWave(int index, double now)
    {
        WaveIndex = index;
        _enemies.Clear();
        foreach (var def in _stage.Waves[index].Enemies)
        {
            var e = new Unit(_nextId++, def, Side.Enemy);
            e.NextActionAt = now + EnemyInterval(e);
            _enemies.Add(e);
        }
        Write(now, $"Wave {index + 1}/{WaveCount}: {string.Join(", ", _enemies.Select(e => e.Name))}");
    }

    private double EnemyInterval(Unit enemy) => enemy.IsBoss
        ? Tuning.BossInterval
        : Tuning.EnemyIntervalMin + _rng.NextDouble() * (Tuning.EnemyIntervalMax - Tuning.EnemyIntervalMin);

    /// <summary>Moves to the next wave, or ends the battle, once one side is wiped out.</summary>
    private void AfterAction(double now)
    {
        if (!Fighting) return;

        if (!AliveChampions.Any())
        {
            Phase = BattlePhase.Lost;
            Write(now, "The squad has fallen. Defeat.");
            return;
        }

        if (AliveEnemies.Any()) return;

        if (WaveIndex + 1 < WaveCount) SpawnWave(WaveIndex + 1, now);
        else
        {
            Phase = BattlePhase.Won;
            Write(now, "Stage cleared. Victory!");
        }
    }

    private void Write(double time, string text) => _log.Add($"[{time,6:0.00}s] {text}");
}
