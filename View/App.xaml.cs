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
        

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Logger.RegisterSink(new ConsoleLoggerSink());
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }

        private static WindowInteropHelper Interop;
        private static HwndSource HwndSource;
        public static IntPtr Handler
        {
            get { return Interop.Handle; }
        }

        public static void RegisterWindow(MainWindow w)
        {
            using (var log = Logger.LogAction("App registering window"))
            {
                Interop = new WindowInteropHelper(w);
                HwndSource = HwndSource.FromHwnd(Handler);
                HwndSource.AddHook(HotKeyManager.OnWin32Message); //This is made in order the WPF app be able to handle win32 messages
                //1- We register the hotkeys in windows
                //2- User press the keyboard shortkcut of the windows hotkey
                //3- Windows send a message as the hotkey is invoked
                //4- We recive the windows message and we handle it in HotKeyManager.OnWin32Message
                //5- We check if the event is the execution of a hotkey (WM_HOTKEY), and if it is, we check if it is one of our shortcuts in order to execute it
            }

        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (HwndSource != null)
            {
                HwndSource.RemoveHook(HotKeyManager.OnWin32Message);
                HwndSource = null;
                HotKeyManager.UnregisterAll();
            }
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
