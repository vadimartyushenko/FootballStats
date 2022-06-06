using System.Text;

namespace FootballStats
{
    public class SequenceResults
    {
        private readonly int MaxSize;

        private List<int> FixedResults { get; }
        private List<List<int>> AllResults { get; set;}
        private List<IntervalResult> IntervalResults { get; set; }
        private int HomeFixedScore { get; set; }
        private int AwayFixedScore { get; set; }
        public SequenceResults(int size, List<int> fixedResults)
        {
            MaxSize = size;
            FixedResults = fixedResults;
        }

        public void Calc(int? min, int? max, double averageHome, double averageAway)
        {
            AllResults = new List<List<int>> {
                FixedResults
            };

            if (min.HasValue && max.HasValue) {
                IntervalResults = new List<IntervalResult>();
                if (FixedResults.Count >= min) {
                    var fixedResult = FixedResults.ToArray();
                    HomeFixedScore = fixedResult[(min.Value - 1)..].Count(x => x == 1);
                    AwayFixedScore = fixedResult[(min.Value - 1)..].Count(x => x == 0);
                }
                AddIntervalResult(FixedResults, min.Value, max.Value, averageHome, averageAway);
            }
            
            void AddIntervalResult(List<int> item, int minGoals, int maxGoals, double homeAv, double awayAv)
            {
                var intervalRes = new List<int>();
                var width = maxGoals - minGoals + 1;
                var startId = item.Count >= minGoals ? minGoals - 1 : -1;
                var endId = item.Count >= maxGoals ? maxGoals : item.Count;
                if (startId < 0)
                    intervalRes = Enumerable.Repeat(-1, width).ToList();
                else {
                    var fullRes = item.ToArray();

                    intervalRes = fullRes[startId..endId].ToList();
                    if (intervalRes.Count < width)
                        intervalRes.AddRange(Enumerable.Repeat(-1, width - intervalRes.Count).ToList());
                }
                var result = new IntervalResult() {
                    GoalSequence = intervalRes,
                    FullSequence = item.Skip(FixedResults.Count - (HomeFixedScore + AwayFixedScore)).ToList(),
                    HomeIntervalScore = intervalRes.Count(x => x == 1),
                    AwayIntervalScore = intervalRes.Count(y => y == 0),
                };
                result.HomeScore = result.FullSequence.Count(x => x == 1);
                result.AwayScore = result.FullSequence.Count(x => x == 0);

                if (IntervalResults.Contains(result)) 
                    return;
                //result.Probability = PoissonDistribution.PMF(result.HomeScore - HomeFixedScore, homeAv) * PoissonDistribution.PMF(result.AwayScore - AwayFixedScore, awayAv);
                IntervalResults.Add(result);
            }

            for (var i = 1; i < MaxSize; i++) {
                var combos = GetAllCombo(i);
                foreach (var item in combos) {
                    item.InsertRange(0, FixedResults);
                    if (min.HasValue && max.HasValue && max.Value > FixedResults.Count)
                        AddIntervalResult(item, min.Value, max.Value, averageHome, averageAway);
                }
                AllResults.AddRange(combos);
            }

            if (IntervalResults == null) 
                return;
            var uniqScore = IntervalResults.Where(x => !x.GoalSequence.Contains(-1)).ToLookup(
                k => string.Join(':', new int[] {k.HomeScore - HomeFixedScore, k.AwayScore - AwayFixedScore}),
                v => v.FullSequence);

            foreach (var item in IntervalResults.Where(x => !x.GoalSequence.Contains(-1))) {
                var score = string.Join(':', new int[] { item.HomeScore - HomeFixedScore, item.AwayScore - AwayFixedScore });
                item.Probability = PoissonDistribution.PMF(item.HomeScore - HomeFixedScore, averageHome) * PoissonDistribution.PMF(item.AwayScore - AwayFixedScore, averageAway) / uniqScore[score].Count();
            }

            foreach (var item in IntervalResults.Where(x => x.GoalSequence.Contains(-1)))
                item.Probability = PoissonDistribution.PMF(item.HomeScore - HomeFixedScore, averageHome) * PoissonDistribution.PMF(item.AwayScore - AwayFixedScore, averageAway);
        }

        public double HomeIntervalWin() => GetProbSum((i, j) => i > j);

        public double AwayIntervalWin() => GetProbSum((i, j) => i < j);

        public double IntervalDraw() => GetProbSum((i, j) => i == j);

        public double NotClosedInterval()
        {
            var uniqScore = new Dictionary<string, double>();
            if (IntervalResults != null)
                foreach (var item in IntervalResults.Where(x => x.GoalSequence.Contains(-1))) {
                    var score = string.Join(':', new int[] { item.HomeScore - HomeFixedScore, item.AwayScore - AwayFixedScore });
                    if (!uniqScore.ContainsKey(score))
                        uniqScore.Add(score, item.Probability);
                }
            return uniqScore.Any() ? uniqScore.Values.Sum() : 0.0;
        }

        #region Help Methods

        private double GetProbSum(Func<int, int, bool> condition)
        {
            var uniqScore = new List<(string, double)>();
            if (IntervalResults != null)
                foreach (var item in IntervalResults.Where(x => !x.GoalSequence.Contains(-1))) {
                    var score = string.Join(':', new int[] { item.HomeScore - HomeFixedScore, item.AwayScore - AwayFixedScore });
                    if (condition(item.HomeIntervalScore, item.AwayIntervalScore))
                        uniqScore.Add((score, item.Probability));

                }
            return uniqScore.Any() ? uniqScore.Sum(x => x.Item2) : 0.0;
        }
        public static List<List<int>> GetAllCombo(int length)
        {
            var result = new List<List<int>>();
            var max = 1 << length;
            for (var i = 0; i < max; i++) {
                var combo = FuncTo2Ext(i, length);
                var comboAdd = combo.Select(x => int.Parse(x.ToString())).ToList();
                result.Add(comboAdd);
            }
            return result;
        }
        private static string FuncTo2Ext(int number, int length)
        {
            var to2 = FuncTo2(number);
            if (to2.Length < length) {
                var leadingZeros = new string(Enumerable.Repeat('0', length - to2.Length).ToArray());
                return to2.Insert(0, leadingZeros);
            }
            return to2;
        }
        private static string FuncTo2(int number)
        {
            return number switch {
                0 => "0",
                1 => "1",
                _ => FuncTo2(number / 2) + (number % 2)
            };
        }
        #endregion
    }

    public class IntervalResult
    {
        public List<int> GoalSequence { get; init;}
        public List<int> FullSequence { get; init;}
        public int HomeScore { get; set;}
        public int AwayScore { get; set;}
        public int HomeIntervalScore { get; init; }
        public int AwayIntervalScore { get; init; }
        public double Probability { get; set;}
        public override bool Equals(object obj)
        {
            if (obj is null or not IntervalResult) 
                return false;
            
            var other = (IntervalResult)obj;
            return other.FullSequence.Count == FullSequence.Count && other.GoalSequence.SequenceEqual(FullSequence);
        }

        public override int GetHashCode() => GoalSequence.GetHashCode();
    }
}