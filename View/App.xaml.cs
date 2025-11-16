using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using debstDevelopments.Common;
using debstDevelopments.Common.Logging;
using debstDevelopments.Common.Logging.Sinks;

namespace VolumeSwitch
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {

        public const int MUTE_HOTKEY_ID = 1;
        public const int VOL_UP_HOTKEY_ID = 2;
        public const int VOL_DOWN_HOTKEY_ID = 3;
        public const int MUTE_MICROPHONE_HOTKEY_ID = 4;

        public static string GetActionName(int actionId)
        {
            switch (actionId)
            {
                case MUTE_HOTKEY_ID:
                    return nameof(MUTE_HOTKEY_ID);
                case VOL_UP_HOTKEY_ID:
                    return nameof(VOL_UP_HOTKEY_ID);
                case VOL_DOWN_HOTKEY_ID:
                    return nameof(VOL_DOWN_HOTKEY_ID);
                case MUTE_MICROPHONE_HOTKEY_ID:
                    return nameof(MUTE_MICROPHONE_HOTKEY_ID);
                default:
                    return string.Empty;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Logger.RegisterSink(new ConsoleLoggerSink());
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }

        private static WindowInteropHelper Interop;
        public static IntPtr Handler
        {
            get { return Interop.Handle; }
        }

        public static void RegisterWindow(MainWindow w)
        {
            using (var log = Logger.LogAction("App registering window"))
            {
                Interop = new WindowInteropHelper(w);
            }

        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            debstDevelopments.HotKeyManager.HotKeyManager.UnregisterAllHotKeys(Handler);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log($"{nameof(OnDispatcherUnhandledException)} {e.Exception?.FullStackTrace()}");
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log($"{nameof(OnDomainUnhandledException)} {e.SerializeJson()}");
                
        }
    }
}
