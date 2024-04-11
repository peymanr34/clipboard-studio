using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ClipboardStudio
{
    internal static partial class NativeMethods
    {
        public static uint GetDpiForWindow(nint hwnd)
        {
            return PInvoke.GetDpiForWindow(new HWND(hwnd));
        }

        public static bool FlashWindowEx(nint hwnd, bool start = true)
        {
            var fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = new HWND(hwnd);

            if (start)
            {
                fInfo.dwFlags = FLASHWINFO_FLAGS.FLASHW_ALL;
                fInfo.uCount = 2;
            }
            else
            {
                fInfo.dwFlags = FLASHWINFO_FLAGS.FLASHW_STOP;
                fInfo.uCount = 0;
            }

            fInfo.dwTimeout = 0;

            return PInvoke.FlashWindowEx(in fInfo);
        }
    }
}
