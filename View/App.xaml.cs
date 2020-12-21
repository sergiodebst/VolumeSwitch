using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace VolumeSwitch
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int MUTE_HOTKEY_ID = 73673;
        public const int VOL_UP_HOTKEY_ID = 73674;
        public const int VOL_DOWN_HOTKEY_ID = 73675;

        private static WindowInteropHelper Interop;
        private static HwndSource HwndSource;
        public static IntPtr Handler
        {
            get { return Interop.Handle; }
        }

        public static void RegisterWindow(MainWindow w)
        {
            Interop = new WindowInteropHelper(w);
            HwndSource = HwndSource.FromHwnd(Handler);
            HwndSource.AddHook(HotKeyManager.HwndHook);
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (HwndSource != null)
            {
                HwndSource.RemoveHook(HotKeyManager.HwndHook);
                HwndSource = null;
                HotKeyManager.UnregisterAll();
            }
        }
    }
}
