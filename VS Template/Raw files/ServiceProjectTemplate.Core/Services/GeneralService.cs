using System.Diagnostics;

namespace $safeprojectname$.Services
{
    public class GeneralService
    {
        /// <summary>
        /// log delegate holder
        /// </summary>
        private LogDelegate _updatelog;

        /// <summary>
        /// thread/process id
        /// </summary>
        private int _taskid;

        public GeneralService(LogDelegate updatelog, int taskid = -1)
        {
            _updatelog = updatelog;
            _taskid = taskid;
        }

        /// <summary>
        /// Hello World demo class. LogDelegate and/or taskid could be passed as parameters here 
        /// </summary>
        /// <param name="includeSpeedTest"></param>
        public void HelloWorld(bool includeSpeedTest = false, int? taskid = null)
        {
            if (taskid.HasValue)
            {
                _taskid = taskid.Value;
            }

            var LOG_TITLE = Helpers.FormatLogTitle("Hello World", _taskid); // add the task id to the title

            _updatelog(LogType.BeginEnd, LOG_TITLE + " - BEGIN");
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Do Hello World stuff here...


            stopwatch.Stop();
            _updatelog(LogType.BeginEnd, LOG_TITLE + " - END");

            if (includeSpeedTest)
                _updatelog(LogType.Highlight, LOG_TITLE + " - Time elapsed: " + stopwatch.Elapsed);
        }
    }
}
