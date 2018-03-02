using System.Diagnostics;

namespace ServiceProjectTemplate.Core.Services
{
    public class GeneralService
    {
        public GeneralService() {}

        /// <summary>
        /// Hello World demo class. LogDelegate and/or taskid could be passed as parameters here 
        /// </summary>
        /// <param name="includeSpeedTest"></param>
        public void HelloWorld(LogDelegate updatelog, int taskid = -1, bool includeSpeedTest = false)
        {
            var LOG_TITLE = Helpers.FormatLogTitle("Hello World", taskid); // add the task id to the title

            updatelog(LogType.BeginEnd, LOG_TITLE + " - BEGIN");
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Do Hello World stuff here...


            stopwatch.Stop();
            updatelog(LogType.BeginEnd, LOG_TITLE + " - END");

            if (includeSpeedTest)
                updatelog(LogType.Highlight, LOG_TITLE + " - Time elapsed: " + stopwatch.Elapsed);
        }
    }
}
