﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Application
{
    public class ApplicationSetting
    {

        /// <summary>
        /// 如果用户改了账户邮箱，并不一定马上要体现在应用里，前提是只在Configure页面设置用户名
        /// 如果每次在MainPage里在线设置用户名，可能会有一次导致和本地设置不一致，从而加载不了文件只能手动下载并重启
        /// 反之，如果他们重新登陆，所有的文件会自动下载到新的账户文件夹里，他们可能不会发现有什么不对。
        /// </summary>
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

        public static object LocalDateModified
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["LocalDateModified"];
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["LocalDateModified"] = value;
            }
        }

        public static void KeyRemove(string key)
            => ApplicationData.Current.LocalSettings.Values.Remove(key);

        public static bool KeyContain(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key) ? true : false;
        }

    }
}
