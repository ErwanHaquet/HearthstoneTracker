namespace HearthCap.StartUp
{
    using System;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Threading;

    using GoogleAnalyticsTracker;

    using HearthCap.Core.GameCapture;
    using HearthCap.Logging;

    using Microsoft.WindowsAPICodePack.ApplicationServices;

    using NLog;

    using Application = System.Windows.Application;
    using Tracker = HearthCap.Features.Analytics.Tracker;

    [Export(typeof(CrashManager))]
    public class CrashManager
    {
        private readonly IAppLogManager appLogManager;

        private readonly static Logger Log = NLog.LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public CrashManager(IAppLogManager appLogManager)
        {
            this.appLogManager = appLogManager;
        }

        public void WireUp()
        {
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += ApplicationOnDispatcherUnhandledException;

            ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings("-died", RestartRestrictions.None));
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            this.HandleException(e.Exception);
        }

        private void ApplicationOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.HandleException(e.Exception);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.HandleException((Exception)e.ExceptionObject);
        }

        public void HandleException(Exception exception)
        {
            Log.Fatal(exception);

            Tracker.TrackEventAsync(Tracker.ErrorsCategory, "Fatal", exception.ToString(), 1);
            appLogManager.Flush();

            //var result = MessageBox.Show("An unhandled error occured. Please report this error.\nRestarting is recommended. Restart now?", "Unhandled error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            //if (result == DialogResult.Yes)
            //{
            //}
            // Environment.Exit(-1);
        }
    }
}