using System;

namespace BuildTimeHistory
{
    public class HistoryRecord
    {
        public DateTime RecordDate { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public int CancelCount { get; set; }
        public double SuccessBuildTime { get; set; }
        public double FailBuildTime { get; set; }
        public double CancelBuildTime { get; set; }

        // This is from when S/F/C times weren't tracked separately
        [Obsolete]
        public double TotalBuildTime { get; set; }

        public int TotalCount
            => SuccessCount + FailCount + CancelCount;

        public bool HasMultipleDifferentResults
            => SuccessCount > 0 && (FailCount > 0 || CancelCount > 0) || FailCount > 0 && CancelCount > 0;

#pragma warning disable CS0612 // Type or member is obsolete
        // Include TBT here so correctly account for days when using old an dnew versions of the extension
        public double CalculatedTotalBuildTime
            => TotalBuildTime + SuccessBuildTime + FailBuildTime + CancelBuildTime;

        public static HistoryRecord CreateNew()
        {
            return new HistoryRecord
            {
                RecordDate = DateTime.Now,
                CancelCount = 0,
                FailCount = 0,
                SuccessCount = 0,
                TotalBuildTime = 0,
                SuccessBuildTime = 0,
                FailBuildTime = 0,
                CancelBuildTime = 0,
            };
        }
    }
}

#pragma warning restore CS0612 // Type or member is obsolete