using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Domain.Entities
{
    public class AnalysisSummary
    {
        public int TotalAll { get; set; }
        public int TotalComplies { get; set; }
        public int TotalDeviates { get; set; }
        public int TotalUnclear { get; set; }
    }
}
