// Plays the sample stage in the terminal with Auto, then prints a results screen.
//
//   dotnet run --project demo/GameModel.Demo                 one battle, seed 1
//   dotnet run --project demo/GameModel.Demo -- --seed 7     a different battle
//   dotnet run --project demo/GameModel.Demo -- --runs 500   balance report over 500 seeds

using GameModel.Battle;
using GameModel.Core;
using GameModel.Data;
using GameModel.Simulation;

int seed = ArgInt("--seed", 1);
int runs = ArgInt("--runs", 0);

string dataDir = FindDataDir();
var champions = DataLoader.ParseChampions(File.ReadAllText(Path.Combine(dataDir, "champions.json")));
var stageFile = DataLoader.ParseStage(File.ReadAllText(Path.Combine(dataDir, "stage-forest-road.json")));
var squad = DataLoader.ResolveSquad(stageFile, champions);

if (runs > 0)
{
    BalanceReport(runs);
    return;
}

var battle = new Battle(stageFile.Stage, squad, new SeededRandom(seed));
var result = BattleSimulator.Run(battle);

Console.WriteLine($"=== {stageFile.Stage.Name} (seed {seed}) ===");
Console.WriteLine($"Squad: {string.Join(", ", squad.Select(c => $"{c.Name} [{c.Element}]"))}");
Console.WriteLine();
foreach (var line in battle.Log) Console.WriteLine(line);
Console.WriteLine();
Console.WriteLine($"Result    : {result.Outcome} in {result.Seconds:0.0} s");
Console.WriteLine($"Survivors : {result.SurvivingChampions}/{squad.Count}");
Console.WriteLine($"Sparks    : {result.Stats.Sparks}");
Console.WriteLine($"Empowers  : {result.Stats.Empowers}");
Console.WriteLine($"Best hit  : {result.Stats.BestHit}");
Console.WriteLine($"Damage    : {result.Stats.DamageDealt} dealt, {result.Stats.DamageTaken} taken");
Console.WriteLine();
foreach (var c in battle.Champions)
    Console.WriteLine($"  {c.Name,-11} {HpBar(c.HpFraction)} {c.Hp,5}/{c.MaxHp}");

void BalanceReport(int count)
{
    var results = Enumerable.Range(0, count)
        .Select(s => BattleSimulator.Run(new Battle(stageFile.Stage, squad, new SeededRandom(s))))
        .ToList();
    var wins = results.Where(r => r.Outcome == BattlePhase.Won).ToList();
    Console.WriteLine($"=== {stageFile.Stage.Name}: {count} Auto runs ===");
    Console.WriteLine($"Win rate        : {100.0 * wins.Count / count:0.0}%");
    if (wins.Count > 0)
    {
        Console.WriteLine($"Avg clear time  : {wins.Average(r => r.Seconds):0.0} s");
        Console.WriteLine($"Avg survivors   : {wins.Average(r => r.SurvivingChampions):0.0}");
    }
    Console.WriteLine($"Avg Sparks      : {results.Average(r => r.Stats.Sparks):0.0}");
    Console.WriteLine($"Avg Empowers    : {results.Average(r => r.Stats.Empowers):0.0}");
}

static string HpBar(float share)
{
    int filled = (int)Math.Round(share * 20);
    return "[" + new string('#', filled) + new string('.', 20 - filled) + "]";
}

int ArgInt(string name, int fallback)
{
    int i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out int v) ? v : fallback;
}

static string FindDataDir()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "data"))) dir = dir.Parent;
    return dir != null ? Path.Combine(dir.FullName, "data") : throw new DirectoryNotFoundException("data/ not found");
}
