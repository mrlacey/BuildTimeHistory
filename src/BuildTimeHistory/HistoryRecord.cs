using System;

namespace BuildTimeHistory
{
    public class HistoryRecord
    {
        public DateTime RecordDate { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public int CancelCount { get; set; }
        public double TotalBuildTime { get; set; }

        public int TotalCount => SuccessCount + FailCount + CancelCount;

        public bool HasMultipleDifferentResults => SuccessCount > 0 && (FailCount > 0 || CancelCount > 0) || FailCount > 0 && CancelCount > 0;

        public static HistoryRecord CreateNew()
        {
            return new HistoryRecord
            {
                RecordDate = DateTime.Now,
                CancelCount = 0,
                FailCount = 0,
                SuccessCount = 0,
                TotalBuildTime = 0,
            };
        }
    }
}
