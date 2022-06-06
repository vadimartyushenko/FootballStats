namespace FootballStats
{
    public class MonteCarloSimulator
    {
        private int NIteration { get; }
        private double HomeAverage { get; }
        private double AwayAverage { get; }

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
            var notClosedCount = 0;

            for (var i = 0; i < NIteration; i++) {
                var home_goals_scored = DistGenerator.GetPoisson(HomeAverage);
                var away_goals_scored = DistGenerator.GetPoisson(AwayAverage);

                if (home_goals_scored + away_goals_scored < needToCLose) {
                    notClosedCount++;
                    continue;
                }

                if (condition == null)
                    continue;

                var test_sequence = SequenceGenerate(home_goals_scored, away_goals_scored).ToList();
                test_sequence.InsertRange(0, fixedResult);

                var intervalSequence = test_sequence.Skip(min - 1).Take(max - min + 1).ToArray();
                var homeIntervalScore = intervalSequence.Sum();
                var awayIntervalScore = intervalSequence.Length - homeIntervalScore;
                if (!condition(homeIntervalScore + homeFixedScore, awayIntervalScore + awayFixedScore)) 
                    continue;
                count++;
                if (log) Console.WriteLine($"#{i}: Home {home_goals_scored + homeFixedScore}:{away_goals_scored + awayFixedScore} Away");
            }
            return condition != null ? (double)count / NIteration : (double)notClosedCount / NIteration;
        }

        private static readonly Random rng = new((int)DateTime.Now.Ticks);

        private static IEnumerable<int> SequenceGenerate(int homeScore, int awayScore)
        {
            var sequence = Enumerable.Repeat(1, homeScore).ToList();
            sequence.AddRange(Enumerable.Repeat(0, awayScore));
            //Fisher–Yates shuffle
            for (var i = sequence.Count - 1; i >= 1; i--) {
                var j = rng.Next(i + 1);
                (sequence[j], sequence[i]) = (sequence[i], sequence[j]);
            }
            return sequence;
        }

        public struct SimulationResults
        {
            public double HomeWinProbability { get; init; }
            public double AwayWinProbability { get; init; }
            public double DrawProbability { get; init; }
        }
    }

}
