using ServiceProjectTemplate.Core;
using ServiceProjectTemplate.Core.Models;
using ServiceProjectTemplate.Core.Services;
using System;
using System.Threading.Tasks;

namespace ServiceProjectTemplate.WPF
{
    public class TaskProcess
    {
        private Services _services = new Services();

        private IProgress<LogModel> _process;

        public TaskProcess(IProgress<LogModel> process)
        {
            _process = process;
        }

        /// <summary>
        /// Testing of xml file loading
        /// </summary>
        /// <returns></returns>
        public async Task RunHelloWorld(bool includeSpeedTest = true)
        {
            await Task.Run(() =>
            {
                _services.General.HelloWorld(UpdateLog, Task.CurrentId ?? -1, includeSpeedTest);
            });
        }

        /// <summary>
        /// Method for log delegate 
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public void UpdateLog(LogType type, string msg = "")
        {
            _process.Report(new LogModel() { LogType = type, Message = msg });
        }
    }
}
