using FootballStats;

var interval = 90;
var pathToStatsFile = args.FirstOrDefault(x => x.StartsWith("-file:"));
if (pathToStatsFile == null) {
    Console.WriteLine("Usage: FootballStats.exe -file:<PATH TO FILE WITH STATS>");
    Console.WriteLine("Other params:");
    Console.WriteLine(" -interval:<remaining minutes> (default: 90 min)");
    Console.WriteLine(" -monte-carlo:<iteration counts>");
    return;
}
var path = pathToStatsFile.Split(new[] { ':' }, 2)[1];
var intervalArg = args.FirstOrDefault(x => x.StartsWith("-interval:"));
if (intervalArg != null) {
    interval = int.Parse(intervalArg.Split(new[] { ':' }, 2)[1]);
    if (interval > 90) {
        Console.WriteLine("Remaining time cannot exceed 90 minutes!");
        return;
    }
}

var nInteration = 10000;
var needSim = false;
var monteCarlo = args.FirstOrDefault(x => x.StartsWith("-monte-carlo"));
if (monteCarlo != null)
    needSim = int.TryParse(monteCarlo.Split(new[] { ':' }, 2)[1], out nInteration);

var stats = new Statistics();
stats.Load(path);
stats.CalcAverage();

Console.WriteLine($"For Home team average scores - {stats.AverageHome}");
Console.WriteLine($"For Away team average scores - {stats.AverageAway}");

var remainPart = interval / 90.0;

var probs = new MatrixResults(7, remainPart * stats.AverageHome, remainPart * stats.AverageAway);

Console.WriteLine(probs);

Console.WriteLine("\n* PMF STATS *\n");
Console.WriteLine($"Home win probability = {probs.HomeWin()}");
Console.WriteLine($"Away win probability = {probs.AwayWin()}");
Console.WriteLine($"Draw probability = {probs.Draw()}");

if (needSim) {
    var simulator = new MonteCarloSimulator(nInteration, remainPart * stats.AverageHome, remainPart * stats.AverageAway);
    simulator.Run(log: false);
    var results = simulator.Results;

    Console.WriteLine("\n* SIM STATS *\n");
    Console.WriteLine($"Home win - {results.HomeWinProbability:F5}, away win - {results.AwayWinProbability:F5}, draw - {results.DrawProbability:F5}");
}