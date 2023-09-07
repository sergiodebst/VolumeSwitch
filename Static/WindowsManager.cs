using debstDevelopments.Common;
using debstDevelopments.Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VolumeSwitch
{
    public class WindowsManager
    {
        static WindowsManager()
        {
            MustHookFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MustHookFileName));
            var file = MustHookFile;
            MustHookFileWatcher = new FileSystemWatcher(file.DirectoryName, file.Name);
            MustHookFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            MustHookFileWatcher.Changed += OnMustHookFileChanged;
            MustHookFileWatcher.Created += OnMustHookFileChanged;
            MustHookFileWatcher.Deleted += OnMustHookFileChanged;
            MustHookFileWatcher.EnableRaisingEvents = true;
        }

        //https://stackoverflow.com/a/10280800
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private static readonly FileSystemWatcher MustHookFileWatcher;
        private static readonly string MustHookFileName = "hook";
        private static FileInfo MustHookFile { get; }
        private static string[] _MustHook;
        private static string[] MustHook
        {
            get
            {
                if (_MustHook == null && MustHookFile.ExistsNow())
                {
                    _MustHook = File.ReadAllLines(MustHookFile.FullName);
                }
                return _MustHook;
            }
        }


        delegate void WinEventHookProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private static WinEventHookProc _handler; //Static so the GC does not clean it and the hook stop working
        private static IntPtr CurrentHook = IntPtr.Zero;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventHookProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hhk);

        private static bool IsCurrentAppActive()
        {
            return App.Handler == GetForegroundWindow();
        }

        private static bool ShouldMonitorActiveAppChange()
        {
            return !IsCurrentAppActive() && !MustHook.IsNullOrEmpty();
        }

        public static void StartMonitoringActiveAppChangesIfNeeded()
        {
            using (var log = Logger.LogAction("Starting active app monitor"))
            {
                if (ShouldMonitorActiveAppChange() && CurrentHook == IntPtr.Zero)
                {
                    _handler = new WinEventHookProc(OnActiveWindowChanged);
                    CurrentHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _handler, 0, 0, WINEVENT_OUTOFCONTEXT);
                    Logger.Log("Active app monitor started");
                }
            }
        }

        public static void StopMonitoringActiveAppChanges()
        {
            using (var log = Logger.LogAction("Stopping active app monitor"))
            {
                if (CurrentHook != IntPtr.Zero)
                {
                    UnhookWinEvent(CurrentHook);
                    CurrentHook = IntPtr.Zero;
                    KeyboardManager.StopKeyboardHook();
                    Logger.Log("Active app monitor stopped");
                }
            }
        }

        private static void OnActiveWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                using (var log = Logger.LogAction("Active app changed"))
                {
                    bool disabled = false;
                    var file = MustHookFile;
                    if (MustHook?.Length > 0)
                    {
                        Logger.Log("Hook file contain entries");
                        uint pid;
                        GetWindowThreadProcessId(hwnd, out pid);
                        Process p = Process.GetProcessById((int)pid);
                        var activeProcessFile = new FileInfo(p.MainModule.FileName);
                        Logger.Log($"Current process is {pid}, {p.MainModule.FileName}, {activeProcessFile.Name} ");
                        if (MustHook.Contains(activeProcessFile.Name))
                        {
                            KeyboardManager.StartKeyboardHook(null);
                            disabled = true;
                        }
                    }
                    if (!disabled && !IsCurrentAppActive())
                    {
                        KeyboardManager.StopKeyboardHook();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.FullStackTrace());
            }
        }

        private static void OnMustHookFileChanged(object sender, FileSystemEventArgs e)
        {
            _MustHook = null;
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (ShouldMonitorActiveAppChange())
                {
                    StartMonitoringActiveAppChangesIfNeeded();
                }
                else
                {
                    StopMonitoringActiveAppChanges();
                }
            }));
        }
    }
}
