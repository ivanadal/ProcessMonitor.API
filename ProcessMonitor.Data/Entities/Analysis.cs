using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Domain.Entities
{
    public class Analysis
    {
        public int Id { get; set; }

        public string Action { get; set; }
        public string Guideline { get; set; }
        public string Result { get; set; } // COMPLIES / VIOLATES
        public double Confidence { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
