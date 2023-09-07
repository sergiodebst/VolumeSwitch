using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//https://stackoverflow.com/a/13139478
namespace VolumeSwitch
{
    public static class VolumneManager
    {
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 0x180000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        
        public static void MicrophoneMute()
        {
            SendMessageW(App.Handler, WM_APPCOMMAND, App.Handler, (IntPtr)APPCOMMAND_MICROPHONE_VOLUME_MUTE);
        }

        public static void Mute(bool toggleMic)
        {
            SendMessageW(App.Handler, WM_APPCOMMAND, App.Handler, (IntPtr)APPCOMMAND_VOLUME_MUTE);
            if (toggleMic) MicrophoneMute();
        }

        public static void VolDown()
        {
            SendMessageW(App.Handler, WM_APPCOMMAND, App.Handler, (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        public static void VolUp()
        {
            SendMessageW(App.Handler, WM_APPCOMMAND, App.Handler, (IntPtr)APPCOMMAND_VOLUME_UP);
        }
    }
}
