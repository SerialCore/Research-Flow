using LogicService.Data;

namespace LogicService.Application
{
    public class ApplicationConfig
    {
        public static void ConfigureDB()
        {
            FileList.DBInitializeTrace();
            FileList.DBInitializeList();
            Feed.DBInitialize();
            Paper.DBInitialize();
            PaperFile.DBInitialize();
        }

        public static void ConfigureVersion()
        {
            ApplicationSetting.Updated = ApplicationVersion.CurrentVersion().ToString();
        }

        public static void ConfigureUpdate()
        {
            // updated version must be greater than the previous published version
            // can be lighter than the next publish version
            if (ApplicationVersion.Parse(ApplicationSetting.Updated) < new ApplicationVersion(3, 42, 108, 0))
            {
                // TODO
                ApplicationSetting.Updated = "3.42.108.0";
            }
        }

    }
}
