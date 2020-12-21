using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VolumeSwitch
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {

        public KeyboardShortcut MuteShortcut
        {
            get
            {
                return this.Config.MuteShortcut;
            }
            set
            {
                this.Config.MuteShortcut = value;
                this.RaisePropertyChanged();
            }
        }

        public KeyboardShortcut VolUpShortcut
        {
            get
            {
                return this.Config.VolUpShortcut;
            }
            set
            {
                this.Config.VolUpShortcut = value;
                this.RaisePropertyChanged();
            }
       }

        public KeyboardShortcut VolDownShortcut
        {
            get
            {
                return this.Config.VolDownShortcut;
            }
            set
            {
                this.Config.VolDownShortcut = value;
                this.RaisePropertyChanged();
            }
        }


        private Config Config;
        private readonly MainWindow Window;
        public MainWindowViewModel(MainWindow w)
        {
            this.Config = new Config();
            this.Window = w;
            this.SaveCommand = new RelayCommand(this.Save);
            this.OpenConfigurationCommand = new RelayCommand(this.OpenConfiguration);
            this.CloseAppCommand = new RelayCommand(this.CloseApp);
            if(this.Config.MuteShortcut != null || this.Config.VolDownShortcut != null || this.Config.VolUpShortcut != null)
            {
                w.WindowState = WindowState.Minimized;
            }
        }

        
        public RelayCommand OpenConfigurationCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand CloseAppCommand { get; set; }

        private void Save()
        {
            this.Config.Save();
        }

        private void OpenConfiguration()
        {
            if (this.Window.IsVisible)
            {
                this.Window.Activate();
            }
            else
            {
                this.Window.Show();
                this.Window.WindowState = WindowState.Normal;
                this.Window.Activate();
            }
        }

        private void CloseApp()
        {
            App.Current.Shutdown();
        }
    }
}
