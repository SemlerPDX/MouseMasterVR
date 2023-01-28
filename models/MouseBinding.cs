using Gma.System.MouseKeyHook;
using MouseMasterVR.Properties;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseMasterVR
{
    public class MouseBinding
    {
        #region param

        private const double DEFAULT_DEBOUNCE = 0.50; //in seconds
        bool[] _previousKeyState = new bool[2];

        #endregion

        private IKeyboardMouseEvents m_GlobalHook;

        public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseWheelExt += GlobalHookMouseWheelExt;
        }
        private async void GlobalHookMouseWheelDebounce(int previousKeyState)
        {
            double mouseDebounce = DEFAULT_DEBOUNCE;
            if (Settings.Default.MouseDebounce >= 0.01)
                mouseDebounce = Settings.Default.MouseDebounce;

            await Task.Delay(TimeSpan.FromSeconds(mouseDebounce));
            _previousKeyState[previousKeyState] = false;

        }

        private void GlobalHookMouseWheelExt(object sender, MouseEventExtArgs e)
        {
            try
            {
                if (Settings.Default.MouseUpRebind.Length == 1 && e.Delta > 0)
                {
                    // Mouse Wheel Scrolled up
                    if (!_previousKeyState[0])
                    {
                        _previousKeyState[0] = true;
                        string upKeyRebind = Settings.Default.MouseUpRebind;
                        SendKeys.SendWait($"{upKeyRebind}");
                        e.Handled = true;
                        GlobalHookMouseWheelDebounce(0);
                    }
                }
                else if (Settings.Default.MouseDownRebind.Length == 1 && e.Delta < 0)
                {
                    // Mouse Wheel Scrolled down
                    if (!_previousKeyState[1])
                    {
                        _previousKeyState[1] = true;
                        string downKeyRebind = Settings.Default.MouseDownRebind;
                        SendKeys.SendWait($"{downKeyRebind}");
                        e.Handled = true;
                        GlobalHookMouseWheelDebounce(1);
                    }
                }
            }
            catch
            {
                // let it slide...
            }
        }

        public void Unsubscribe()
        {
            if (m_GlobalHook != null)
            {
                m_GlobalHook.MouseWheelExt -= GlobalHookMouseWheelExt;

                //  Trash it....
                m_GlobalHook.Dispose();
            }
        }
    }
}
