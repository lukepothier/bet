namespace Bet
{
    internal static class StringExtensions
    {
        public static string Clean(this string input)
            => input?.Trim().Replace(',', '.');
    }
}
