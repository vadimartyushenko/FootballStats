using System.Collections;
using System.Text;

namespace FootballStats
{
    public static class SequenceFormatter
    {
        const int MaxSize = 7;
        public static List<int> FormatFromInput(string goals)
        {
            var result = new List<int>();
            foreach (var goal in goals) {
                var item = goal switch {
                    'H' => 1,
                    'A' => 0,
                    _ => throw new Exception("Not supported symbol in goal string")
                };
                result.Add(item);
            }
            return result;
        }

        public static void PrintAllResultsTable(IEnumerable<List<int>> results)
        {
            Console.WriteLine("\n* RESULTS TABLE *\n");
            foreach (var result in results) {
                var str = string.Join(" - ", result.Select(x => x == 1 ? "H" : "A"));
                Console.WriteLine(" " + str);
            }
        }
    }
}
