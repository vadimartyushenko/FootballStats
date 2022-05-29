using System.Text;

namespace FootballStats
{
    public class SequenceResults
    {
        public readonly int MaxSize;
        
        public List<int> FixedResults { get; private set; }
        public List<List<int>> AllResults { get; private set;}
        public List<IntervalResult> IntervalResults { get; private set; }
        public SequenceResults(int size, List<int> fixedResults)
        {
            MaxSize = size;
            FixedResults = fixedResults;
        }

        public void Calc(int? min, int? max)
        {
            //not used yet
            var homeMax = FixedResults.Count(x => x == 1);
            var awayMax = FixedResults.Count(x => x == 0);

            AllResults = new List<List<int>> {
                FixedResults
            };

            if (min.HasValue && max.HasValue) {
                IntervalResults = new List<IntervalResult>();
                AddIntervalResult(FixedResults, min.Value, max.Value);
            }
            
            void AddIntervalResult(List<int> item, int min, int max)
            {
                var intervalRes = new List<int>();
                var width = max - min + 1;
                var startId = item.Count >= min ? min - 1 : -1;
                var endId = item.Count >= max ? max : item.Count;
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
                if (!IntervalResults.Contains(result))
                    IntervalResults.Add(result);
            }

            for (var i = 1; i < MaxSize; i++) {
                var combos = GetAllCombo(i);
                foreach (var item in combos) {
                    item.InsertRange(0, FixedResults);
                    if (min.HasValue && max.HasValue && max.Value > FixedResults.Count)
                        AddIntervalResult(item, min.Value, max.Value);
                }
                AllResults.AddRange(combos);
            }
        }

        public void PrintIntervalResults()
        {
            if (IntervalResults == null)
                return;
            Console.WriteLine("\n* INTERVAL RESULTS TABLE *\n");
            //headers
            Console.WriteLine($"{"Goal Sequence", -30}\t{"Score", 5}\t{"Result", 10}");
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
                    result = "Interval not completed";
                else if (item.HomeScore > item.AwayScore)
                    result = "Home Win";
                else if (item.HomeScore < item.AwayScore)
                    result = "Away Win";
                else
                    result = "Draw";

                Console.WriteLine($"{sb}\t{item.HomeScore, 2}:{item.AwayScore, -2}\t{result, 10}");
            }
        }
        #region Help Methods
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
        public List<int> GoalSequence { get; set;}
        public int HomeScore { get; set;}
        public int AwayScore { get; set;}

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not IntervalResult) return false;
            IntervalResult other = (IntervalResult)obj;
            if (other.GoalSequence.Count != this.GoalSequence.Count)
                return false;
            if (!other.GoalSequence.SequenceEqual(this.GoalSequence))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return GoalSequence.GetHashCode();
        }
    }
}