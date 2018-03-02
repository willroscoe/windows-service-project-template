namespace ServiceProjectTemplate.Core.Services
{
    public class Services
    {
        public Services()
        {
            General = new GeneralService();
        }

        public GeneralService General { get; set; }
    }
}
