using CsvHelper;
using CsvHelper.Configuration;
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
            IEnumerable<Match> matches = LoadMatches(@"Assets\results.csv");

            IEnumerable<Coordinate> coordinates = matches
                .Select(m => new Coordinate
                {
                    X = m.ResultImpliedProbabilityExclVigorish(),
                    Y = (int)m.AbsoluteMargin
                });

            RegressionResult regressionResult = LinearRegression(coordinates);

            Match match = GetMatchInputs();

            double predictedValue = PredictMargin(regressionResult, match.PredictedWinnerImpliedProbabilityExclVigorish());

            Console.WriteLine();

            Console.WriteLine($"{match.PredictedWinnerName().FormatMatchResult()} is predicted with a margin of {Math.Round(predictedValue)}.");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Prompt for input match data
        /// </summary>
        /// <returns>A Match model based on the user inputs</returns>
        private static Match GetMatchInputs()
        {
            Console.WriteLine("What is the first team called?");
            string team1Name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(team1Name))
            {
                Exit("Input was null or empty, exiting...");
            }

            Console.WriteLine("What is the other team called?");
            string team2Name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(team2Name))
            {
                Exit("Input was null or empty, exiting...");
            }

            Console.WriteLine($"Predicting for {team1Name} vs {team2Name}!");
            Console.WriteLine("Please Enter all odds in European format (decimal), e.g. \"1.23\".");

            Console.WriteLine($"What are the odds on {team1Name} to win?");
            string input = Console.ReadLine();
            if (!double.TryParse(input.Clean(), NumberStyles.Float, CultureInfo.InvariantCulture, out double team1Odds))
            {
                Exit($"\"{input}\" was not parsable to a double, exiting...");
            }

            Console.WriteLine($"What are the odds on {team2Name} to win?");
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

        /// <summary>
        /// Reads the input .csv of bookmaker odds and match results
        /// </summary>
        /// <returns>A set of Match models</returns>
        private static IEnumerable<Match> LoadMatches(string path)
        {
            var matches = new List<Match>();

            try
            {
                using (TextReader reader = File.OpenText(path))
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
            matches.RemoveAll(m => !m.IsWinnerPredictionCorrect());

            return matches;
        }

        /// <summary>
        /// Performs simple linear regression to output an R-squared, Y-intercept, and slope of the line of best fit
        /// X values should be the independent variable
        /// </summary>
        /// <param name="coordinates">The collection of cartesian coordinates</param>
        private static RegressionResult LinearRegression(IEnumerable<Coordinate> coordinates)
        {
            double sumX = 0d;
            double sumY = 0d;
            double sumXSquared = 0d;
            double sumYSquared = 0d;
            double sumCodeviates = 0d;

            foreach (Coordinate coordinate in coordinates)
            {
                double x = coordinate.X;
                double y = coordinate.Y;

                sumCodeviates += x * y;
                sumX += x;
                sumY += y;
                sumXSquared += x * x;
                sumYSquared += y * y;
            }

            int count = coordinates.Count();
            double ssX = sumXSquared - ((sumX * sumX) / count);

            double rNumerator = (count * sumCodeviates) - (sumX * sumY);
            double rDenominator = (count * sumXSquared - (sumX * sumX)) * (count * sumYSquared - (sumY * sumY));
            double sCo = sumCodeviates - ((sumX * sumY) / count);

            double meanX = sumX / count;
            double meanY = sumY / count;
            double dblR = rNumerator / Math.Sqrt(rDenominator);

            return new RegressionResult
            {
                RSquared = dblR * dblR,
                YIntercept = meanY - ((sCo / ssX) * meanX),
                Slope = sCo / ssX
            };
        }

        /// <summary>
        /// Predicts a margin of victory
        /// </summary>
        /// <param name="regressionResult">Linear regression model</param>
        /// <param name="impliedProbabilityExclVigorish">Implied probability of victory excl. vigorish</param>
        /// <returns>Predicted margin of victory</returns>
        private static double PredictMargin(RegressionResult regressionResult, double impliedProbabilityExclVigorish)
            => (regressionResult.Slope * impliedProbabilityExclVigorish) + regressionResult.YIntercept;

        /// <summary>
        /// Exits the program
        /// </summary>
        /// <param name="exitMessage">The message to print before exiting</param>
        static void Exit(string exitMessage)
        {
            Console.WriteLine(exitMessage);
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}
