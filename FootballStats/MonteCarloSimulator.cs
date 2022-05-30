namespace FootballStats
{
    public class MonteCarloSimulator
    {
        public int NIteration { get; }
        public double HomeAverage { get; }
        public  double AwayAverage { get; }

        public SimulationResults Results { get; private set; }

        public MonteCarloSimulator(int nIteration, double homeAverage, double awayAverage)
        {
            NIteration = nIteration;
            HomeAverage = homeAverage;
            AwayAverage = awayAverage;
        }

        public void Run(bool log = false)
        {
            var count_home_wins = 0;
            var count_away_wins = 0;
            var count_draws = 0;
            for (var i = 0; i < NIteration; i++)
            {
                var home_goals_scored = DistGenerator.GetPoisson(HomeAverage);
                var away_goals_scored = DistGenerator.GetPoisson(AwayAverage);

                if (home_goals_scored > away_goals_scored)
                    count_home_wins++;
                else if (home_goals_scored < away_goals_scored)
                    count_away_wins++;
                else
                    count_draws++;
                if (log)
                    Console.WriteLine($"#{i}: Home {home_goals_scored}:{away_goals_scored} Away");
            }

            //calculate probabilities to win/lose/draw
            Results = new SimulationResults()
            {
                HomeWinProbability = (double) count_home_wins / NIteration,
                AwayWinProbability = (double) count_away_wins / NIteration,
                DrawProbability = (double) count_draws / NIteration
            };
        }

        public double Run(int max, int min, List<int> fixedResult, Func<int, int, bool> condition, bool log)
        {
            var homeFixedScore = 0;
            var awayFixedScore = 0;
            if (fixedResult.Count >= min) {
                var fixedResultArray = fixedResult.ToArray();
                homeFixedScore = fixedResultArray[(min - 1)..].Count(x => x == 1);
                awayFixedScore = fixedResultArray[(min - 1)..].Count(x => x == 0);
            }

            var needToCLose = max - min + 1 - (homeFixedScore + awayFixedScore);
            var count = 0;

            for (var i = 0; i < NIteration; i++) {
                var home_goals_scored = DistGenerator.GetPoisson(HomeAverage);
                var away_goals_scored = DistGenerator.GetPoisson(AwayAverage);

                if (home_goals_scored + away_goals_scored != needToCLose)
                    continue;
                if (condition(home_goals_scored + homeFixedScore, away_goals_scored + awayFixedScore))
                {
                    count++;
                    if (log)
                        Console.WriteLine($"#{i}: Home {home_goals_scored + homeFixedScore}:{away_goals_scored + awayFixedScore} Away");
                }
            }

            return (double)count / NIteration;
        }

        public struct SimulationResults
        {
            public double HomeWinProbability { get; init; }
            public double AwayWinProbability { get; init; }
            public double DrawProbability { get; init; }
        }
    }

}
