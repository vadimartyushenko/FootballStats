using System.Text;

namespace FootballStats
{
    public class SequenceResults
    {
        private readonly int MaxSize;
        
        public List<int> FixedResults { get; }
        public List<List<int>> AllResults { get; private set;}
        public List<IntervalResult> IntervalResults { get; private set; }
        public int HomeFixedScore { get; private set; } = 0;
        public int AwayFixedScore { get; private set; } = 0;
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
                    HomeScore = intervalRes.Count(x => x == 1),
                    AwayScore = intervalRes.Count(y => y == 0)
                };

                if (IntervalResults.Contains(result)) 
                    return;
                result.Probability = PoissonDistribution.PMF(result.HomeScore - HomeFixedScore, homeAv) * PoissonDistribution.PMF(result.AwayScore - AwayFixedScore, awayAv);
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

        public void PrintIntervalResults()
        {
            if (IntervalResults == null)
                return;
            Console.WriteLine("\n* INTERVAL RESULTS TABLE *\n");
            //headers
            Console.WriteLine($"{"Goal Sequence", -20}\t{"Score", 5}\t{"Result", 10}\t{"Probability", 10}");
            foreach (var item in IntervalResults) {
                var sb = new StringBuilder();
                foreach (var goal in item.GoalSequence) {
                    var symbol = goal switch {
                        1 => "H",
                        0 => "A",
                        _ => "*"
                    };
                    sb.Append(symbol);
                    sb.Append(" - ");
                }

                string result;
                if (item.GoalSequence.Contains(-1))
                    result = "Not compl.";
                else if (item.HomeScore > item.AwayScore)
                    result = "Home Win";
                else if (item.HomeScore < item.AwayScore)
                    result = "Away Win";
                else
                    result = "Draw";

                Console.WriteLine($"{sb}\t{item.HomeScore, 2}:{item.AwayScore, -2}\t{result, 10}\t {item.Probability:F5}");
            }
        }

        #region Help Methods

        private double GetProbSum(Func<int, int, bool> condition)
        {
            var uniqScore = new Dictionary<string, double>();
            if (IntervalResults != null)
                foreach (var item in IntervalResults.Where(x => !x.GoalSequence.Contains(-1))) {
                    var score = string.Join(':', new int[] { item.HomeScore - HomeFixedScore, item.AwayScore - AwayFixedScore });
                    if(!uniqScore.ContainsKey(score) && condition(item.HomeScore, item.AwayScore))
                        uniqScore.Add(score, item.Probability);
                }
            return uniqScore.Any() ? uniqScore.Values.Sum() : 0.0;
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
        public int HomeScore { get; init;}
        public int AwayScore { get; init;}
        public double Probability { get; set;}
        public override bool Equals(object obj)
        {
            if (obj is null or not IntervalResult) 
                return false;
            
            var other = (IntervalResult)obj;
            return other.GoalSequence.Count == GoalSequence.Count && other.GoalSequence.SequenceEqual(GoalSequence);
        }

        public override int GetHashCode() => GoalSequence.GetHashCode();
    }
}