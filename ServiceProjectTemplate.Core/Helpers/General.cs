using System;

namespace ServiceProjectTemplate.Core
{
    /// <summary>
    /// Delegate for logging across multiple project types and log outputs. Eg. the WPF project outputs the log messages to the screen. The windows service project outputs to log files
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    /// <param name="percent"></param>
    public delegate void LogDelegate(LogType type, String message = "");


    public class Helpers
    {
        /// <summary>
        /// This adds the task id to the title, if it is >= 0. Used in the Logging
        /// </summary>
        /// <param name="title"></param>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public static string FormatLogTitle(string title, int taskid)
        {
            if (taskid < 0)
                return title;

            return string.Format("{0} ({1})", title, taskid);
        }
    }
}
