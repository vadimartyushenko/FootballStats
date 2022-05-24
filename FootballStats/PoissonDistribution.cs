namespace FootballStats
{
    public static class PoissonDistribution
    {
        public static double PMF(int k, double lambda) => Math.Pow(lambda, k) * Math.Exp(-lambda) / Factorial(k);

        private static int Factorial(int number) => number <= 1 ? 1 : number * Factorial(number - 1);
    }
}
