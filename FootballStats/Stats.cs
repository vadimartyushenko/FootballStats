using System.Runtime.InteropServices.ComTypes;

namespace FootballStats
{
    public struct StatItem
    {
        public int ScoreHome;
        public int ScoreAway;
        public float Percent;
    }

    public class Statistics
    {
        public List<StatItem> Items { get; } = new ();
        public double AverageHome { get; private set; }
        public double AverageAway { get; private set; }

        public void Load(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("File with stats not found!", nameof(path));

            using var reader = new StreamReader(path);
            reader.ReadLine();//skip headers
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                var values = line?.Split(';');
                if (values?.Length == 3)
                {
                    Items.Add(new StatItem()
                    {
                        ScoreHome = int.Parse(values[0]),
                        ScoreAway = int.Parse(values[1]),
                        Percent = float.Parse(values[2])
                    });
                }
            }

        }

        public void CalcAverage()
        {
            var averHome = 0.0f;
            var averAway = 0.0f;
            foreach (var item in Items)
            {
                averHome += item.ScoreHome * item.Percent / 100;
                averAway += item.ScoreAway * item.Percent / 100;
            }
            AverageHome = averHome;
            AverageAway = averAway;
        }
    }
}
