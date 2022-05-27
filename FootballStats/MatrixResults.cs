using System.Data.Common;
using System.Text;

namespace FootballStats
{
    public class MatrixResults
    {
        private readonly int _size;
        private readonly double[,] _probs;

        public MatrixResults(int size, double homeAverage, double awayAverage)
        {
            if (size <= 0)
                throw new ArgumentException("Incorrect size of probs matrix", nameof(size));
            _size = size;
            _probs =  new double[_size, _size];

            for (var i = 0; i < _size; i++) 
                for (var j = 0; j < _size; j++) 
                    _probs[i, j] = PoissonDistribution.PMF(i, homeAverage) * PoissonDistribution.PMF(j, awayAverage);
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendLine($"    {string.Join(" ", Enumerable.Range(0, _size).Select(x => $"   [{x}]   "))}");
            for (var i = 0; i < _size; i++)
            {
                s.Append($"[{i}]");
                for (var j = 0; j < _size; j++)
                    s.Append($"  {_probs[i,j]:F5} ");
                s.AppendLine();
            }
            return s.ToString();
        }

    }
}
