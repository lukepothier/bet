using CsvHelper.Configuration.Attributes;
using System;
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
        public int? Team1Score { get; set; }

        [Name("Away score")]
        public int? Team2Score { get; set; }

        [Name("Home odds")]
        public double Team1Odds { get; set; }

        [Name("Away odds")]
        public double Team2Odds { get; set; }

        [Name("Draw odds")]
        public double DrawOdds { get; set; }

        public int? Margin => Team1Score - Team2Score;

        public int? AbsoluteMargin => Margin is null ? Margin : Math.Abs((int)Margin);

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

        public double ResultImpliedProbabilityExclVigorish
        {
            get
            {
                return IsTeam1Win
                    ? Team1ImpliedProbabilityExclVigorish
                    : IsTeam2Win
                        ? Team2ImpliedProbabilityExclVigorish
                        : DrawImpliedProbabilityExclVigorish;
            }
        }

        public bool WinnerPredictionCorrect
        {
            get
            {
                bool predictedCorrectly = false;

                IList<double> ImpliedProbabilites = new List<double> { Team1ImpliedProbabilityInclVigorish, Team2ImpliedProbabilityInclVigorish, DrawImpliedProbabilityInclVigorish };

                if (ImpliedProbabilites.ElementAt(0) >= ImpliedProbabilites.ElementAt(1) && ImpliedProbabilites.ElementAt(0) >= ImpliedProbabilites.ElementAt(2) && IsTeam1Win)
                    predictedCorrectly = true;
                else if (ImpliedProbabilites.ElementAt(1) >= ImpliedProbabilites.ElementAt(0) && ImpliedProbabilites.ElementAt(1) >= ImpliedProbabilites.ElementAt(2) && IsTeam2Win)
                    predictedCorrectly = true;
                else if (ImpliedProbabilites.ElementAt(2) >= ImpliedProbabilites.ElementAt(0) && ImpliedProbabilites.ElementAt(2) >= ImpliedProbabilites.ElementAt(1) && IsDraw)
                    predictedCorrectly = true;

                return predictedCorrectly;
            }
        }

        public string PredictedWinnerName
        {
            get
            {
                IList<double> ImpliedProbabilites = new List<double> { Team1ImpliedProbabilityInclVigorish, Team2ImpliedProbabilityInclVigorish, DrawImpliedProbabilityInclVigorish };

                if (ImpliedProbabilites.ElementAt(0) >= ImpliedProbabilites.ElementAt(1) && ImpliedProbabilites.ElementAt(0) >= ImpliedProbabilites.ElementAt(2))
                    return Team1Name;
                else if (ImpliedProbabilites.ElementAt(1) >= ImpliedProbabilites.ElementAt(0) && ImpliedProbabilites.ElementAt(1) >= ImpliedProbabilites.ElementAt(2))
                    return Team2Name;
                else
                    return "Draw";
            }
        }

        public double PredictedWinnerImpliedProbabilityExclVigorish
        {
            get
            {
                IList<double> ImpliedProbabilites = new List<double> { Team1ImpliedProbabilityInclVigorish, Team2ImpliedProbabilityInclVigorish, DrawImpliedProbabilityInclVigorish };

                return (100d / TotalImpliedProbabilityInclVigorish) * ImpliedProbabilites.Max();
            }
        }
    }
}
