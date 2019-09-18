using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Bet
{
    internal class Program
    {
        private static void Main()
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
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double homeOddsEuropean))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            Console.WriteLine($"What are the odds on {team2Name}?");
            input = Console.ReadLine();
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double awayOddsEuropean))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            Console.WriteLine("What are the odds on a draw?");
            input = Console.ReadLine();
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double drawOddsEuropean))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            var homeImpliedProbability = homeOddsEuropean.ToImpliedProbability();
            var awayImpliedProbability = awayOddsEuropean.ToImpliedProbability();
            var drawImpliedProbability = drawOddsEuropean.ToImpliedProbability();

            var matches = new List<Match>();

            try
            {
                using (TextReader reader = File.OpenText(@"Assets\results.csv"))
                {
                    var csv = new CsvReader(reader);
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
            matches.RemoveAll(m => !m.WinnerPredictionCorrect());

            var team1Wins = matches.Where(m => m.IsTeam1Win);
            var team2Wins = matches.Where(m => m.IsTeam2Win);
            var draws = matches.Where(m => m.IsDraw);

            var correctCount = matches.Count(m => m.WinnerPredictionCorrect());

            var totalResultImpliedProbabilityExclVigorish = 0d;
            var totalMargin = 0;

            foreach (var match in matches)
            {
                totalResultImpliedProbabilityExclVigorish += match.ResultImpliedProbabilityExclVigorish();
                totalMargin += Math.Abs(match.Margin);

                Console.WriteLine($"Match {match.Team1Name} vs {match.Team2Name}:");
                Console.WriteLine($"The result's IP excl. vigorish was {match.ResultImpliedProbabilityExclVigorish()}.");
                Console.WriteLine($"The actual margin was {Math.Abs(match.Margin)}");
                Console.WriteLine();
            }

            Console.WriteLine($"Average result's IP excl. vigorish was {totalResultImpliedProbabilityExclVigorish / matches.Count}.");
            Console.WriteLine($"Average margin was {totalMargin / matches.Count}.");

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void Exit(string exitMessage)
        {
            Console.WriteLine(exitMessage);
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}
