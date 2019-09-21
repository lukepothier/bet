using System;

namespace Bet
{
    internal static class StringExtensions
    {
        public static string Clean(this string input)
            => input?.Trim().Replace(',', '.');

        public static string FormatMatchResult(this string input)
        {
            if (input.Equals("Draw", StringComparison.OrdinalIgnoreCase))
                return "A draw";

            return $"Victory for {input}";
        }
    }
}
