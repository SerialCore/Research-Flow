using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Power;
using Windows.Services.Store;
using Windows.System;
using Windows.System.Diagnostics;

namespace LogicService.Application
{
    public class ApplicationInfo
    {

        public static string AppName => SystemInformation.Instance.ApplicationName;

        public static string AppVersion => $"{SystemInformation.Instance.ApplicationVersion.Major}." +
            $"{SystemInformation.Instance.ApplicationVersion.Minor}." +
            $"{SystemInformation.Instance.ApplicationVersion.Build}." +
            $"{SystemInformation.Instance.ApplicationVersion.Revision}";

        public static CultureInfo Culture => SystemInformation.Instance.Culture;

        public static string OperatingSystem => SystemInformation.Instance.OperatingSystem;

        public static ProcessorArchitecture OperatingSystemArchitecture => SystemInformation.Instance.OperatingSystemArchitecture;

        public static OSVersion OperatingSystemVersion => SystemInformation.Instance.OperatingSystemVersion;

        public static string DeviceFamily => SystemInformation.Instance.DeviceFamily;

        public static string DeviceModel => SystemInformation.Instance.DeviceModel;

        public static string DeviceManufacturer => SystemInformation.Instance.DeviceManufacturer;

        public static float AvailableMemory => SystemInformation.Instance.AvailableMemory;

        public static bool IsFirstUse => SystemInformation.Instance.IsFirstRun;

        public static bool IsAppUpdated => SystemInformation.Instance.IsAppUpdated;

        public static string FirstVersionInstalled => SystemInformation.Instance.FirstVersionInstalled.ToFormattedString();

        public static DateTime FirstUseTime => SystemInformation.Instance.FirstUseTime;

        public static DateTime LaunchTime => SystemInformation.Instance.LaunchTime;

        public static DateTime LastLaunchTime => SystemInformation.Instance.LastLaunchTime;

        public static DateTime LastResetTime => SystemInformation.Instance.LastResetTime;

        public static long LaunchCount => SystemInformation.Instance.LaunchCount;

        public static long TotalLaunchCount => SystemInformation.Instance.TotalLaunchCount;

        public static TimeSpan AppUptime => SystemInformation.Instance.AppUptime;

        public static void TrackAppUse(LaunchActivatedEventArgs args)
        {
            SystemInformation.Instance.TrackAppUse(args);
        }

        public static async Task<bool> ShowRatingReviewDialog()
        {
            StoreSendRequestResult result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, String.Empty);
            if (result.ExtendedError == null)
            {
                JObject jsonObject = JObject.Parse(result.Response);
                if (jsonObject.SelectToken("status").ToString() == "success")
                {
                    // The customer rated or reviewed the app.
                    return true;
                }
            }
            return false;
        }

        public static bool IsNetworkAvailable => NetworkInterface.GetIsNetworkAvailable();

        public static ulong MemoryUsage => MemoryManager.AppMemoryUsage / 1024 / 1024;

        private static TimeSpan oldcputime = TimeSpan.Zero;
        private static TimeSpan oldprotime = TimeSpan.Zero;

        public static double CpuOccupation
        {
            get
            {
                var proreport = ProcessDiagnosticInfo.GetForCurrentProcess().CpuUsage.GetReport();
                var cpureport = SystemDiagnosticInfo.GetForCurrentSystem().CpuUsage.GetReport();

                TimeSpan addcputime = cpureport.UserTime + cpureport.KernelTime - oldcputime;
                TimeSpan addprotime = proreport.UserTime + proreport.KernelTime - oldprotime;
                oldcputime = cpureport.UserTime + cpureport.KernelTime;
                oldprotime = proreport.UserTime + proreport.KernelTime;
                double rate = (double)addprotime.Ticks / addcputime.Ticks;
                return rate;
            }
        }

        public static double BatteryUsage
        {
            get
            {
                var report = Battery.AggregateBattery.GetReport();
                if (report.RemainingCapacityInMilliwattHours.HasValue && report.FullChargeCapacityInMilliwattHours.HasValue)
                    return (double)report.RemainingCapacityInMilliwattHours.Value / report.FullChargeCapacityInMilliwattHours.Value;
                else
                    return 0;
            }
        }

    }
}
