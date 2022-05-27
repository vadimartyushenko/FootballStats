using FootballStats;

var nInteration = 1000;
var stats = new Statistics();
stats.Load(@"Stats.csv");
stats.CalcAverage();

Console.WriteLine($"For Home team average scores - {stats.AverageHome}");
Console.WriteLine($"For Away team average scores - {stats.AverageAway}");

var interval = (90 - 1800 / 60.0) / 90; 
Console.WriteLine(PoissonDistribution.PMF(0, interval * stats.AverageHome));

var probs = new MatrixResults(5, interval * stats.AverageHome, interval * stats.AverageAway);

Console.WriteLine(probs);

var simulator = new MonteCarloSimulator(nInteration, interval * stats.AverageHome, interval * stats.AverageAway);
simulator.Run(log:false);
var results = simulator.Results;

Console.WriteLine("* SIM STATS *");
Console.WriteLine($"Home win - {results.HomeWinProbability:F}, away win - {results.AwayWinProbability:F}, draw - {results.DrawProbability:F}");
