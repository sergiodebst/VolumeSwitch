using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using debstDevelopments.Common.Logging;

namespace VolumeSwitch
{
    public static class KeyboardManager
    {
        public class KeyEvent
        {
            public KeyboardShortcut Key { get; private set; }
            public bool Handled { get; set; }

            public KeyEvent(KeyboardShortcut key)
            {
                this.Key = key;
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBHookStruct lParam);
        private static LowLevelKeyboardProc _handler; //Static so the GC does not clean it and the hook stop working
        private static IntPtr CurrentHook = IntPtr.Zero;
        private static IEnumerable<IHandleKeyboardHookControl> HandlerControls;
        private const int WH_KEYBOARD_LL = 13;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KBHookStruct lParam);

        //The utility of the keyboardhook is to be the first app to get the keyboard events
        //We want to be the first on handling the keyboard event for 2 reasons
        //1- In our own application we want to write the pressed shortcut in the configuration boxes without trigger any other hotkey
        //2- There are some other apps that hooks the keyboard events, and our configured hotkeys could be never triggered, so when we know one of 
        //these application is being executed (these applications are specified in the hook file), we hook them before so no one can deny us the execution of the hotkeys
        public static void StartKeyboardHook(IEnumerable<IHandleKeyboardHookControl> controls)
        {
            using (var log = Logger.LogAction($"{nameof(StartKeyboardHook)}"))
            {
                if (CurrentHook == IntPtr.Zero)
                {
                    // Note: This does not work in the VS host environment.  To run in debug mode:
                    // Project -> Properties -> Debug -> Uncheck "Enable the Visual Studio hosting process"
                    IntPtr hInstance = Marshal.GetHINSTANCE(typeof(App).Module);
                    HandlerControls = controls;
                    _handler = new LowLevelKeyboardProc(KeyboardHookHandler);
                    CurrentHook = SetWindowsHookEx(WH_KEYBOARD_LL, _handler, hInstance, 0);
                    Logger.Log("System keys disabled");
                }
            }
        }


        public static void StopKeyboardHook()
        {
            using (var log = Logger.LogAction($"{nameof(StopKeyboardHook)}"))
            {
                if (CurrentHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(CurrentHook);
                    HandlerControls = null;
                    CurrentHook = IntPtr.Zero;
                    Logger.Log("System keys enabled");
                }
            }
        }

        private static List<Key> CurrentlyPressedKeys = new List<Key>();
        private static IntPtr KeyboardHookHandler(int nCode, IntPtr wParam, ref KBHookStruct lParam)
        {
            KeyboardShortcut key = null;
            if (nCode == 0)
            {
                var wpfKey = KeyInterop.KeyFromVirtualKey(lParam.vkCode);
                var wparamTyped = wParam.ToInt32();
                if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
                {
                    var isKeyDown = false;
                    if (wparamTyped == (int)KeyboardState.WM_KEYDOWN || wparamTyped == (int)KeyboardState.WM_SYSKEYDOWN)
                    {
                        isKeyDown = true;
                        if (!CurrentlyPressedKeys.Contains(wpfKey)) CurrentlyPressedKeys.Add(wpfKey);
                    }
                    else if (CurrentlyPressedKeys.Contains(wpfKey))
                    {
                        CurrentlyPressedKeys.Remove(wpfKey);
                    }
                    else
                    {
                        var aaa = wparamTyped;
                    }

                    key = new KeyboardShortcut(wpfKey)
                    {
                        CtrlModifier = CurrentlyPressedKeys.Contains(Key.LeftCtrl) || CurrentlyPressedKeys.Contains(Key.LeftCtrl),
                        ShiftModifier = CurrentlyPressedKeys.Contains(Key.LeftShift) || CurrentlyPressedKeys.Contains(Key.RightShift),
                        AltModifier = CurrentlyPressedKeys.Contains(Key.LeftAlt) || CurrentlyPressedKeys.Contains(Key.RightAlt),
                        WinModifier = CurrentlyPressedKeys.Contains(Key.LWin) || CurrentlyPressedKeys.Contains(Key.RWin)
                    };
                    var keyEvent = new KeyEvent(key);

                    if (HandlerControls != null)
                    {
                        foreach (var control in HandlerControls)
                        {
                            if (control.CanHandleHook)
                            {
                                control.HandleHook(keyEvent);
                                if (keyEvent.Handled) break;
                            }
                        }
                    }

                    if (keyEvent.Handled)
                    {
                        return new IntPtr(1);
                    }
                    else if (!keyEvent.Handled && isKeyDown)
                    {
                        KeyboardShortcutAction action = HotKeyManager.CallActionIfRegistered(key);
                        if (action != null) return new IntPtr(1);
                    }
                }
            }

            Logger.Log($"Calling next hook for {nCode} {wParam.ToInt32()} { KeyInterop.KeyFromVirtualKey(lParam.vkCode)} ({key.ToString()})");
            return CallNextHookEx(CurrentHook, nCode, wParam, ref lParam);
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct KBHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public enum KeyboardState : int
        {
            WM_KEYDOWN = 256,
            WM_KEYUP = 257,
            WM_SYSKEYUP = 261,
            WM_SYSKEYDOWN = 260
        }
    }
}
