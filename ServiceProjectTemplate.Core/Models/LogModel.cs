using System;

namespace ServiceProjectTemplate.Core.Models
{
    /// <summary>
    /// Holder for logging messages
    /// </summary>
    public class LogModel
    {
        public LogType LogType { get; set; }
        public string Message { get; set; }
    }
}
