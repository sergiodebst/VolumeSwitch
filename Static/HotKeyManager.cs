using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using debstDevelopments.Common.Logging;

namespace VolumeSwitch
{
    public static class HotKeyManager
    {

        public const int MUTE_HOTKEY_ID = 1;
        public const int VOL_UP_HOTKEY_ID = 2;
        public const int VOL_DOWN_HOTKEY_ID = 3;

        private static string GetActionName(int actionId)
        {
            switch (actionId)
            {
                case MUTE_HOTKEY_ID:
                    return nameof(MUTE_HOTKEY_ID);
                case VOL_UP_HOTKEY_ID:
                    return nameof(VOL_UP_HOTKEY_ID);
                case VOL_DOWN_HOTKEY_ID:
                    return nameof(VOL_DOWN_HOTKEY_ID);
                default:
                    return string.Empty;
            }
        }

        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static Dictionary<int, KeyboardShortcutAction> RegisteredActions = new Dictionary<int, KeyboardShortcutAction>();
        public static void Register(KeyboardShortcutAction action)
        {
            if (RegisteredActions.ContainsKey(action.Id))
            {
                Unregister(action.Id);
            }
            var win32Key = KeyInterop.VirtualKeyFromKey(action.Shortcut.Key);
            // Modifier keys codes: Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            // Compute the addition of each combination of the keys you want to be pressed
            // ALT+CTRL = 1 + 2 = 3 , CTRL+SHIFT = 2 + 4 = 6...
            if (!RegisterHotKey(App.Handler, action.Id, action.Modifiers(), (int)win32Key))
            {
                MessageBox.Show($"Cannot use hotkey {action.Shortcut.ToString()}, probably windows or other application is using it.");
            }
            RegisteredActions[action.Id] = action;
            Logger.Log($"Action registered for {GetActionName(action.Id)} on ({action.Shortcut.ToString()})");
        }

        private static void Unregister(int actionId)
        {
            using (var log = Logger.LogAction($"Unregister {GetActionName(actionId)}."))
            {
                UnregisterHotKey(App.Handler, actionId);
                RegisteredActions.Remove(actionId);
                Logger.Log($"Unregistered {GetActionName(actionId)}");
            }
        }

        public static void UnregisterAll()
        {
            foreach (var action in RegisteredActions.Values.ToList())
            {
                Unregister(action.Id);
            }
        }

        public static KeyboardShortcutAction CallActionIfRegistered(KeyboardShortcut key)
        {
            var action = RegisteredActions.Values.FirstOrDefault((hotKey) => hotKey.Shortcut.ToString() == key.ToString());
            if (action != null)
            {
                Logger.Log($"Action found for ({key.ToString()})");
                action.Action.Invoke();
                Logger.Log("Action executed.");
                return action;
            }
            return null;
        }

        public static IntPtr OnWin32Message(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //This method is a candidate to be moved somewhere else and do some refactoring
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int actionId = wParam.ToInt32();
                if (RegisteredActions.ContainsKey(actionId))
                {
                    Logger.Log($"Action found for ({GetActionName(actionId)})");
                    var action = RegisteredActions[actionId];
                    action.Action.Invoke();
                    Logger.Log($"Action executed");
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
    }
}
