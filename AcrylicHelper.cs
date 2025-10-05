using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace APPID
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttribData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWCP_ROUND = 2;
        public const int WCA_ACCENT_POLICY = 19;
        public const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;
    }

    public static class AcrylicHelper
    {
        public static void ApplyAcrylic(Form form, bool roundedCorners = true)
        {
            // Apply rounded corners (Windows 11)
            if (roundedCorners)
            {
                try
                {
                    int preference = NativeMethods.DWMWCP_ROUND;
                    NativeMethods.DwmSetWindowAttribute(form.Handle, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
                }
                catch { }
            }

            // Apply acrylic blur effect
            try
            {
                var accent = new AccentPolicy();
                accent.AccentState = NativeMethods.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.AccentFlags = ThemeConfig.BlurIntensity;
                accent.GradientColor = ThemeConfig.AcrylicBlurColor;

                int accentStructSize = Marshal.SizeOf(accent);
                IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttribData();
                data.Attribute = NativeMethods.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;

                NativeMethods.SetWindowCompositionAttribute(form.Handle, ref data);
                Marshal.FreeHGlobal(accentPtr);
            }
            catch { }
        }
    }
}
