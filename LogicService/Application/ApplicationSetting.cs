using Windows.Storage;

namespace LogicService.Application
{
    public class ApplicationSetting
    {
        public static string AccountName
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["AccountName"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["AccountName"] = value;
            }
        }

        public static string Configured
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["Configured"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["Configured"] = value;
            }
        }

        public static string Updated
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["Updated"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["Updated"] = value;
            }
        }

        public static void RemoveKey(string key)
            => ApplicationData.Current.LocalSettings.Values.Remove(key);

        public static bool ContainKey(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key) ? true : false;
        }

        public static bool EqualKey(string key, object value)
        {
            if (!ContainKey(key))
                return false;
            
            if (ApplicationData.Current.LocalSettings.Values[key] == value)
                return true;
            else
                return false;
        }

    }
}
