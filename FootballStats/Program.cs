using FootballStats;

var stats = new Statistics();
stats.Load(@"Stats.csv");
stats.CalcAverage();

Console.WriteLine($"For Home team average scores - {stats.AverageHome}");
Console.WriteLine($"For Away team average scores - {stats.AverageAway}");

var interval = (90 - 1800 / 60.0) / 90; 
Console.WriteLine(PoissonDistribution.PMF(0, interval * stats.AverageHome));

Console.WriteLine(DistGenerator.GetPoisson(1.2));
