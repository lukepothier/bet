namespace Bet
{
    internal static class DoubleExtensions
    {
        public static double ToImpliedProbability(this double europeanOdds)
            => (1d / europeanOdds) * 100d;
    }
}
