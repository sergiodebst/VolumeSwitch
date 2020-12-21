using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VolumeSwitch.KeyboardManager;

namespace VolumeSwitch
{
    public interface IHandleKeyboardHookControl
    {
        bool CanHandleHook { get; }
        void HandleHook(KeyEvent e);
    }
}
