using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Globalization;
using Windows.System;

namespace LogicService.Services
{
    public class SystemInfo
    {

        /// <summary>
        /// To get application's name
        /// </summary>
        public static string ApplicationName => SystemInformation.ApplicationName;

        /// <summary>
        /// To get application's version
        /// </summary>
        public static string ApplicationVersion => $"{SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";

        /// <summary>
        /// To get the most preferred language by the user
        /// </summary>
        public static CultureInfo Culture => SystemInformation.Culture;

        /// <summary>
        /// To get operating syste
        /// </summary>
        public static string OperatingSystem => SystemInformation.OperatingSystem;

        /// <summary>
        /// To get used processor architecture
        /// </summary>
        public static ProcessorArchitecture OperatingSystemArchitecture => SystemInformation.OperatingSystemArchitecture;

        /// <summary>
        /// To get operating system version
        /// </summary>
        public static OSVersion OperatingSystemVersion => SystemInformation.OperatingSystemVersion;

        /// <summary>
        /// To get device family
        /// </summary>
        public static string DeviceFamily => SystemInformation.DeviceFamily;

        /// <summary>
        /// To get device model
        /// </summary>
        public static string DeviceModel => SystemInformation.DeviceModel;

        /// <summary>
        /// To get device manufacturer
        /// </summary>
        public static string DeviceManufacturer => SystemInformation.DeviceManufacturer;

        /// <summary>
        /// To get available memory in MB
        /// </summary>
        public static float AvailableMemory => SystemInformation.AvailableMemory;

        /// <summary>
        /// To get if the app is being used for the first time since it was installed
        /// </summary>
        public static bool IsFirstUse => SystemInformation.IsFirstRun;

        /// <summary>
        /// To get if the app is being used for the first time since being upgraded from an older version
        /// </summary>
        public static bool IsAppUpdated => SystemInformation.IsAppUpdated;

        /// <summary>
        /// To get the first version installed
        /// </summary>
        public static string FirstVersionInstalled => SystemInformation.FirstVersionInstalled.ToFormattedString();

        /// <summary>
        /// To get the first time the app was launched
        /// </summary>
        public static DateTime FirstUseTime => SystemInformation.FirstUseTime;

        /// <summary>
        /// To get the time the app was launched
        /// </summary>
        public static DateTime LaunchTime => SystemInformation.LaunchTime;

        /// <summary>
        /// To get the time the app was previously launched, not including this instance
        /// </summary>
        public static DateTime LastLaunchTime => SystemInformation.LastLaunchTime;

        /// <summary>
        /// To get the number of times the app has been launched
        /// </summary>
        public static long LaunchCount => SystemInformation.LaunchCount;

        /// <summary>
        /// To get how long the app has been running
        /// </summary>
        public static TimeSpan AppUptime => SystemInformation.AppUptime;

    }
}
