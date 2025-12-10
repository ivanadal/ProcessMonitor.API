using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Domain.Entities
{
    public class HuggingFaceResult
    {
        public string Label { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
