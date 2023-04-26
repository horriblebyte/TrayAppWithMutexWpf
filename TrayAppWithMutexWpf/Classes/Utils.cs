using System;
using System.Windows;
using System.Windows.Interop;

namespace TrayAppWithMutexWpf.Classes {
    public static class Utils {

        /// <summary>
        /// //Hali hazırda çalışmakta olan Window nesnesini ekrana getirir.
        /// </summary>
        public static void ShowActiveWindow() {
            _ = NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// İstenen Window nesnesine odaklanır ve ekrana getirir.
        /// </summary>
        /// <param name="window">İşlem yapılacak Window nesnesi</param>
        public static void GlobalActivateWindow(Window window) {
            //Get the process ID for this window's thread
            WindowInteropHelper interopHelper = new WindowInteropHelper(window);
            uint thisWindowThreadId = NativeMethods.GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

            //Get the process ID for the foreground window's thread
            IntPtr currentForegroundWindow = NativeMethods.GetForegroundWindow();
            uint currentForegroundWindowThreadId = NativeMethods.GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

            //Attach this window's thread to the current window's thread
            _ = NativeMethods.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

            //Set the window position
            _ = NativeMethods.SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_SHOWWINDOW);

            //Detach this window's thread from the current window's thread
            _ = NativeMethods.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

            //Show and activate the window
            if (window.WindowState == WindowState.Minimized) {
                window.WindowState = WindowState.Normal;
            }
            window.Show();
            _ = window.Activate();
        }

    }
}