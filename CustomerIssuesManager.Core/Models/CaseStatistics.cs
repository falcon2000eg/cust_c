using System;

namespace CustomerIssuesManager.Core.Models
{
    public class CaseStatistics
    {
        public int TotalCases { get; set; }
        public int NewCases { get; set; }
        public int InProgressCases { get; set; }
        public int SolvedCases { get; set; }
        public int ClosedCases { get; set; }
        public int ActiveCases { get; set; }
        public int CasesThisMonth { get; set; }
        public int CasesThisYear { get; set; }
        public double AverageResolutionTime { get; set; }
        public string MostCommonCategory { get; set; } = string.Empty;
        public string MostActiveEmployee { get; set; } = string.Empty;
        public DateTime LastCaseDate { get; set; }
        public int TotalAttachments { get; set; }
        public int TotalCorrespondences { get; set; }
    }
} 