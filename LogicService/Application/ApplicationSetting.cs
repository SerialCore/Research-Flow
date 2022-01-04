using Windows.Storage;

namespace LogicService.Application
{
    public class ApplicationSetting
    {

        public static string Theme
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["Theme"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["Theme"] = value;
            }
        }

        public static string InkInput
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["InkInput"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["InkInput"] = value;
            }
        }

        public static string LiveTile
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["LiveTile"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["LiveTile"] = value;
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

            if (ApplicationData.Current.LocalSettings.Values[key].Equals(value))
                return true;
            else
                return false;
        }

    }
}
