using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VolumeSwitch.View
{
    public partial class MuteIndicatorWindow : Window, INotifyPropertyChanged
    {
        public bool IsMicrophoneMuted
        {
            get
            {
                var isMuted = VolumneManager.GetMicrophoneIsMuted();
                Trace.WriteLine(isMuted.ToString());
                return isMuted;
            }
        }
        private DispatcherTimer Timer;
        private DoubleAnimation FadeOutAnimation;
        private MuteIndicatorWindow()
        {
            this.DataContext = this;
            this.FadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(1000));
            this.FadeOutAnimation.Completed += this.OnFadeOutComplete;
            this.Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1800), DispatcherPriority.Normal, this.OnTimerTick, this.Dispatcher);
            InitializeComponent();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            this.BeginAnimation(Window.OpacityProperty, this.FadeOutAnimation);
        }

        private void OnFadeOutComplete(object sender, EventArgs e)
        {
            var trullyCompleted = this.Opacity == 0;
            if (trullyCompleted)
            {
                _currentWindow = null;
                this.Close();
                this.FadeOutAnimation.Completed -= this.OnFadeOutComplete;
            }
        }

        private void ResetEverything()
        {
            this.BeginAnimation(Window.OpacityProperty, null);
            this.Opacity = 1;
            this.Timer.Stop();
            this.Timer.Start();
            this.RaisePropertyChanged(nameof(this.IsMicrophoneMuted));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region "Native"
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_TOOLWINDOW = 0x80;
        const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            //To make the window ignore clicks
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        }
        #endregion

        private static MuteIndicatorWindow _currentWindow;
        public static void ShowIndicator()
        {
            _currentWindow?.ResetEverything();
            if (_currentWindow != null) return;

            _currentWindow = new MuteIndicatorWindow();
            _currentWindow.Show();
            _currentWindow.Timer.Start();
        }
    }
}
