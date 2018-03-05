namespace $safeprojectname$.Services
{
    public class Services
    {
        public Services(LogDelegate updatelog, int taskid = -1)
        {
            General = new GeneralService(updatelog, taskid);
        }

        public GeneralService General { get; set; }
    }
}
