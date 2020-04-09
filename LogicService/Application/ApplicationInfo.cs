using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
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

        public static string AppName => SystemInformation.ApplicationName;

        public static string AppVersion => $"{SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";

        public static CultureInfo Culture => SystemInformation.Culture;

        public static string OperatingSystem => SystemInformation.OperatingSystem;

        public static ProcessorArchitecture OperatingSystemArchitecture => SystemInformation.OperatingSystemArchitecture;

        public static OSVersion OperatingSystemVersion => SystemInformation.OperatingSystemVersion;

        public static string DeviceFamily => SystemInformation.DeviceFamily;

        public static string DeviceModel => SystemInformation.DeviceModel;

        public static string DeviceManufacturer => SystemInformation.DeviceManufacturer;

        public static float AvailableMemory => SystemInformation.AvailableMemory;

        public static bool IsFirstUse => SystemInformation.IsFirstRun;

        public static bool IsAppUpdated => SystemInformation.IsAppUpdated;

        public static string FirstVersionInstalled => SystemInformation.FirstVersionInstalled.ToFormattedString();

        public static DateTime FirstUseTime => SystemInformation.FirstUseTime;

        public static DateTime LaunchTime => SystemInformation.LaunchTime;

        public static DateTime LastLaunchTime => SystemInformation.LastLaunchTime;

        public static DateTime LastResetTime => SystemInformation.LastResetTime;

        public static long LaunchCount => SystemInformation.LaunchCount;

        public static long TotalLaunchCount => SystemInformation.TotalLaunchCount;

        public static TimeSpan AppUptime => SystemInformation.AppUptime;

        public static void TrackAppUse(LaunchActivatedEventArgs args)
        {
            SystemInformation.TrackAppUse(args);
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
