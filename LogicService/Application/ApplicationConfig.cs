using LogicService.Data;
using LogicService.Storage;
using System.Threading.Tasks;

namespace LogicService.Application
{
    public class ApplicationConfig
    {

        public static async Task ConfigurePath()
        {
            await LocalStorage.GetDataFolderAsync();
            await LocalStorage.GetLogFolderAsync();
            await LocalStorage.GetNoteFolderAsync();
            await LocalStorage.GetPaperFolderAsync();
        }

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
                Paper.DBUpdateApp();
                ApplicationSetting.Updated = "3.42.108.0";
            }
        }

    }
}
