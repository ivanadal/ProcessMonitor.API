using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when an error occurs during analysis.
    /// </summary>
    public class AnalysisServiceException : Exception
    {
        public AnalysisServiceException()
        {
        }

        public AnalysisServiceException(string message)
            : base(message)
        {
        }

        public AnalysisServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
