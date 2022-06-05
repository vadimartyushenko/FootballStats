using FootballStats;

var pathToStatsFile = args.FirstOrDefault(x => x.StartsWith("-file:"));
if (pathToStatsFile == null) {
    Console.WriteLine("Usage: FootballStats.exe -file:<PATH TO FILE WITH STATS>");
    Console.WriteLine("Other params:");
    Console.WriteLine(" -interval:<remaining minutes> (default: 90 min)");
    Console.WriteLine(" -monte-carlo:<iteration counts>");
    Console.WriteLine(" -goal-sequence:<for example, HAHH> (H - home team goal, A - away team goal)");
    Console.WriteLine(" -min:<min goals in interval>");
    Console.WriteLine(" -max:<max goals in interval>");
    return;
}
var path = pathToStatsFile.Split(new[] { ':' }, 2)[1];

var interval = 90;
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

var sequenceArg = args.FirstOrDefault(x => x.StartsWith("-goal-sequence"));
SequenceResults results = null;
var sequenceStr = string.Empty;
int? minGoals = null;
int? maxGoals = null;
if (sequenceArg != null) {
    sequenceStr = sequenceArg.Split(new[] { ':' }, 2)[1];
    results = new SequenceResults(7, SequenceFormatter.FormatFromInput(sequenceStr));
    
    var minArg = args.FirstOrDefault(x => x.StartsWith("-min"));
    var maxArg = args.FirstOrDefault(x => x.StartsWith("-max"));
    minGoals = minArg != null ? int.Parse(minArg.Split(new[] { ':' }, 2)[1]) : null;
    maxGoals = maxArg != null ? int.Parse(maxArg.Split(new[] { ':' }, 2)[1]) : null;
    if (minGoals is < 1) {
        Console.WriteLine("Args \"min\" must be: min >= 1");
        return;
    }
    if (maxGoals is < 2) {
        Console.WriteLine("Args \"max\" must be: max > 1");
        return;
    }
    if (maxGoals <= minGoals) {
        Console.WriteLine("Args \"min\" and \"max\" must be: min < max");
        return;
    }
}
var stats = new Statistics();
stats.Load(path);
stats.CalcAverage();
Console.WriteLine("\n* STATS FROM FILE *\n");
Console.WriteLine($"For Home team average scores - {stats.AverageHome}");
Console.WriteLine($"For Away team average scores - {stats.AverageAway}");

var remainPart = interval / 90.0;
if (results != null && maxGoals.HasValue && minGoals.HasValue)
{
    results.Calc(minGoals, maxGoals, remainPart * stats.AverageHome, remainPart * stats.AverageAway);
    SequenceFormatter.PrintAllResultsTable(results.AllResults);
    results.PrintIntervalResults();

    Console.WriteLine("\n* INTERVAL PROBABILITY *\n");
    Console.WriteLine($"Home win interval probability = {results.HomeIntervalWin():F5}");
    Console.WriteLine($"Away win interval probability = {results.AwayIntervalWin():F5}");
    Console.WriteLine($"Draw interval probability = {results.IntervalDraw():F5}");
    Console.WriteLine($"Not closed interval probability = {results.NotClosedInterval():F5}");
}

Console.WriteLine("\n* ALL PROBABILITY MATRIX *\n");
var probs = new MatrixResults(7, remainPart * stats.AverageHome, remainPart * stats.AverageAway);
Console.WriteLine(probs);

Console.WriteLine("\n* PMF STATS *\n");
Console.WriteLine($"Home win probability = {probs.HomeWin()}");
Console.WriteLine($"Away win probability = {probs.AwayWin()}");
Console.WriteLine($"Draw probability = {probs.Draw()}");

if (needSim) {
    var simulator = new MonteCarloSimulator(nInteration, remainPart * stats.AverageHome, remainPart * stats.AverageAway);
    simulator.Run(log: false);
    var simResults = simulator.Results;

    Console.WriteLine("\n* SIM STATS *\n");
    Console.WriteLine($"Home win - {simResults.HomeWinProbability:F5}, away win - {simResults.AwayWinProbability:F5}, draw - {simResults.DrawProbability:F5}");

    if (results != null && maxGoals.HasValue && minGoals.HasValue)
    {
        Console.WriteLine("\n* SIM INTERVAL STATS *\n");
        //home win probability
        //var probability = simulator.Run(maxGoals.Value, minGoals.Value, SequenceFormatter.FormatFromInput(sequenceStr), (i, j) => (i == j), true);
        var probability = simulator.Run(maxGoals.Value, minGoals.Value, SequenceFormatter.FormatFromInput(sequenceStr), false);
        Console.WriteLine($"Home interval win probability - {probability:F5}");
    }
}