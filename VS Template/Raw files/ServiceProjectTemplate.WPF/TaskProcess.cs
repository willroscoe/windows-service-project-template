using $ext_projectname$.Core;
using $ext_projectname$.Core.Models;
using $ext_projectname$.Core.Services;
using System;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    public class TaskProcess
    {
        /// <summary>
        /// Services holder
        /// </summary>
        private Services _services;

        private IProgress<LogModel> _process;

        public TaskProcess(IProgress<LogModel> process)
        {
            _process = process;
            _services = new Services(UpdateLog, Task.CurrentId ?? -1);
        }

        /// <summary>
        /// Task to run the Hellow World method. Using tasks uses different threads to the desktop ui so it will not hang
        /// </summary>
        /// <returns></returns>
        public async Task RunHelloWorld(bool includeSpeedTest = true)
        {
            await Task.Run(() =>
            {
                _services.General.HelloWorld(includeSpeedTest, Task.CurrentId ?? -1);
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
