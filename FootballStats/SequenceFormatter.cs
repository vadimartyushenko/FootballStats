namespace FootballStats
{
    public static class SequenceFormatter
    {
        public static List<int> Format(string goals)
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
    }
}
