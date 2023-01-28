using F4SharedMem;
using F4SharedMem.Headers;
using MouseMasterVR.Properties;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MouseMasterVR
{
    public class MouseCentering
    {
        #region param

        private const int POLLING_INTERVAL = 50; //in mss

        // Constants for the GetSystemMetrics method
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        int centerX = 1;
        int centerY = 1;

        // Get the dimensions of the primary screen
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);

        private bool isCentering = false;

        #endregion

        #region user32_helpers

        // Helper method to get the handle of the foreground window
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Helper method to get the dimensions of the primary screen
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        // Helper method to set the mouse position
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        #endregion


        private System.Timers.Timer textUpdateTimer = new System.Timers.Timer(POLLING_INTERVAL);


        public void Initialize(CancellationToken token)
        {
            // Calculate the center point of the screen
            centerX = screenWidth / 2;
            centerY = screenHeight / 2;

            // Create Timer to update TextBlock display
            textUpdateTimer.Elapsed += (s, ea) => CenterMouseCursor(token);
            textUpdateTimer.Start();
        }

        public void CenterMouseCursor(CancellationToken token)
        {
            // Keep the mouse pointer in the center of the screen indefinitely
            if (Settings.Default.MainPowerSwitch)
            {
                if (CanCenterMouse())
                {
                    SetCursorPos(centerX, centerY);
                }
            }

            // ...unless instructed to cancel by token
            if (token.IsCancellationRequested)
                textUpdateTimer.Stop();

        }

        private bool TargetProcessInFocus()
        {
            // Check for valid target process, or revert to default
            if (String.IsNullOrEmpty(Settings.Default.GameTargetName.Trim()))
                Settings.Default.GameTargetName = MainWindowViewModel.DEFAULT_PROCESS;

            // Check if the target process has focus
            Process[] processes = System.Diagnostics.Process.GetProcessesByName(Settings.Default.GameTargetName.Trim());
            try
            {
                if (processes.Length > 0 && !(processes[0].MainWindowHandle == GetForegroundWindow()))
                {
                    return false;
                }
            }
            catch
            {
                // Just return false on any unlikely problems
                return false;
            }
            finally
            {
                // Dispose of any/all process objects
                foreach (System.Diagnostics.Process process in processes)
                {
                    process.Dispose();
                }
            }
            return true;
        }

        private bool IsFlyingFalconBMS()
        {
            try
            {
                using (Reader sharedMemReader = new Reader())
                {
                    if (sharedMemReader != null)
                    {
                        FlightData currentMemData = sharedMemReader.GetCurrentData();
                        if (currentMemData != null)
                        {
                            HsiBits hsiBitsData = (HsiBits)currentMemData.hsiBits;
                            string[] hsiBitsCheck = hsiBitsData.ToString().Split(',').Select(p => p.Trim()).ToArray();
                            if (hsiBitsCheck.Contains("Flying"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // do nothing
            }
            return false;
        }

        private bool CanCenterMouse()
        {
            isCentering = TargetProcessInFocus();
            if (isCentering && Settings.Default.GameTargetName == MainWindowViewModel.DEFAULT_PROCESS)
            {
                isCentering = IsFlyingFalconBMS();
            }
            return isCentering;
        }
    }
}