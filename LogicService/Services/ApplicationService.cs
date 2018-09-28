﻿using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Services.Store;
using Windows.System;

namespace LogicService.Services
{
    public class ApplicationService
    {

        #region Info

        public static string ApplicationName => SystemInformation.ApplicationName;

        public static string ApplicationVersion => $"{SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";

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

        #endregion

        public static async Task LaunchFeedbackAsync()
            => await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();

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

        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type taskEntryPoint,
            string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser
                || status == BackgroundAccessStatus.DeniedByUser)
            {
                return null;
            }

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint.FullName
            };

            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();
            return task;
        }

    }
}
