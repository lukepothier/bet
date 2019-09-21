using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Bet
{
    internal class Program
    {
        private static void Main()
        {
            // LoadMatches();

            var match = GetMatchInputs();

            PresentResults(match);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static Match GetMatchInputs()
        {
            Console.WriteLine("What is the first team called?");
            var team1Name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(team1Name))
            {
                Exit("Input was null or empty, exiting...");
            }

            Console.WriteLine("What is the other team called?");
            var team2Name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(team2Name))
            {
                Exit("Input was null or empty, exiting...");
            }

            Console.WriteLine($"Predicting for {team1Name} vs {team2Name}!");
            Console.WriteLine("Please Enter all odds in European format (decimal), e.g. \"1.23\".");

            Console.WriteLine($"What are the odds on {team1Name}?");
            var input = Console.ReadLine();
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double team1Odds))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            Console.WriteLine($"What are the odds on {team2Name}?");
            input = Console.ReadLine();
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double team2Odds))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            Console.WriteLine("What are the odds on a draw?");
            input = Console.ReadLine();
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double drawOdds))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            return new Match
            {
                Team1Name = team1Name,
                Team2Name = team2Name,
                Team1Odds = team1Odds,
                Team2Odds = team2Odds,
                DrawOdds = drawOdds
            };
        }

        private static void PresentResults(Match match)
        {
            Console.WriteLine();

            Console.WriteLine($"{match.Team1Name} implied probability excl. vigorish: {match.Team1ImpliedProbabilityExclVigorish}%");
            Console.WriteLine($"{match.Team2Name} implied probability excl. vigorish: {match.Team2ImpliedProbabilityExclVigorish}%");
            Console.WriteLine($"Draw implied probability excl. vigorish: {match.DrawImpliedProbabilityExclVigorish}%");

            Console.WriteLine();

            Console.WriteLine($"{match.PredictedWinnerName.FormatMatchResult()} is predicted with implied probability {match.PredictedWinnerImpliedProbabilityExclVigorish}% excl. vigorish.");
        }

        private static void LoadMatches()
        {
            var matches = new List<Match>();

            try
            {
                using (TextReader reader = File.OpenText(@"Assets\results.csv"))
                {
                    var csv = new CsvReader(reader, new Configuration(CultureInfo.InvariantCulture));
                    csv.Configuration.Delimiter = ",";
                    while (csv.Read())
                    {
                        Match Record = csv.GetRecord<Match>();
                        matches.Add(Record);
                    }
                }
            }
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
            {
#if DEBUG
                throw;
#endif
                Exit("Something went wrong parsing the input .csv, exiting...");
#pragma warning restore CS0162 // Unreachable code detected
#pragma warning restore CS0168 // Variable is declared but never used
            }

            // We only care about matches where the winner was correctly predicted by the bookmakers
            matches.RemoveAll(m => !m.WinnerPredictionCorrect);

            var totalResultImpliedProbabilityExclVigorish = 0d;
            var totalMargin = 0;

            foreach (Match m in matches)
            {
                totalResultImpliedProbabilityExclVigorish += m.ResultImpliedProbabilityExclVigorish;
                totalMargin += m.AbsoluteMargin ?? 0;

                Debug.WriteLine($"Match {m.Team1Name} vs {m.Team2Name}:");
                Debug.WriteLine($"The result's IP excl. vigorish was {m.ResultImpliedProbabilityExclVigorish}.");
                Debug.WriteLine($"The actual margin was {m.AbsoluteMargin ?? 0}");
                Debug.WriteLine(Environment.NewLine);
            }
        }

        static void Exit(string exitMessage)
        {
            Console.WriteLine(exitMessage);
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}
