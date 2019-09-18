using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Bet
{
    internal class Match
    {
        [Name("Home team")]
        public string Team1Name { get; set; }

        [Name("Away team")]
        public string Team2Name { get; set; }

        [Name("Home score")]
        public int Team1Score { get; set; }

        [Name("Away score")]
        public int Team2Score { get; set; }

        [Name("Home odds")]
        public double Team1Odds { get; set; }

        [Name("Away odds")]
        public double Team2Odds { get; set; }

        [Name("Draw odds")]
        public double DrawOdds { get; set; }

        public int Margin => Team1Score - Team2Score;

        public bool IsTeam1Win => Margin > 0;

        public bool IsTeam2Win => Margin < 0;

        public bool IsDraw => Margin == 0;

        public double Team1ImpliedProbabilityInclVigorish => Team1Odds.ToImpliedProbability();

        public double Team2ImpliedProbabilityInclVigorish => Team2Odds.ToImpliedProbability();

        public double DrawImpliedProbabilityInclVigorish => DrawOdds.ToImpliedProbability();

        public double TotalImpliedProbabilityInclVigorish
            => Team1ImpliedProbabilityInclVigorish + Team2ImpliedProbabilityInclVigorish + DrawImpliedProbabilityInclVigorish;

        public double Team1ImpliedProbabilityExclVigorish => (100d / TotalImpliedProbabilityInclVigorish) * Team1ImpliedProbabilityInclVigorish;

        public double Team2ImpliedProbabilityExclVigorish => (100d / TotalImpliedProbabilityInclVigorish) * Team2ImpliedProbabilityInclVigorish;

        public double DrawImpliedProbabilityExclVigorish => (100d / TotalImpliedProbabilityInclVigorish) * DrawImpliedProbabilityInclVigorish;

        public double ResultImpliedProbabilityExclVigorish()
        {
            return IsTeam1Win
                ? Team1ImpliedProbabilityExclVigorish
                : IsTeam2Win
                    ? Team2ImpliedProbabilityExclVigorish
                    : DrawImpliedProbabilityExclVigorish;
        }

        public bool WinnerPredictionCorrect()
        {
            bool predictedCorrectly = false;

            IList<double> ImpliedProbabilites = new List<double> { Team1ImpliedProbabilityInclVigorish, Team2ImpliedProbabilityInclVigorish, DrawImpliedProbabilityInclVigorish };

            // TODO :: This is bad, if two implied probabilites happen to be equal we could get false positives
            if (ImpliedProbabilites.Max() == Team1ImpliedProbabilityInclVigorish && IsTeam1Win)
                predictedCorrectly = true;
            else if (ImpliedProbabilites.Max() == Team2ImpliedProbabilityInclVigorish && IsTeam2Win)
                predictedCorrectly = true;
            else if (ImpliedProbabilites.Max() == DrawImpliedProbabilityInclVigorish && IsDraw)
                predictedCorrectly = true;

            return predictedCorrectly;
        }
    }
}
