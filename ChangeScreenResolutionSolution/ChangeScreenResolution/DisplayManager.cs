using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChangeScreenResolution
{
    static class DisplayManager
    {
         /// <summary>
        /// Returns a DisplaySettings object encapsulates the current display settings.
        /// </summary>
        public static DisplaySettings GetCurrentSettings()
        {
            return CreateDisplaySettingsObject(-1, GetDeviceMode());
        }

        /// <summary>
        /// Changes the current display settings with the new settings provided. May throw InvalidOperationException if failed. Check the exception message for more details.
        /// </summary>
        /// <param name="set">The new settings.</param>
        /// <remarks>
        /// Internally calls ChangeDisplaySettings() native function.
        /// </remarks>
        public static void SetDisplaySettings(DisplaySettings set)
        {
            SafeNativeMethods.DEVMODE mode = GetDeviceMode();

            mode.dmPelsWidth = (uint)set.Width;
            mode.dmPelsHeight = (uint)set.Height;
            mode.dmDisplayOrientation = (uint)set.Orientation;
            mode.dmBitsPerPel = (uint)set.BitCount;
            mode.dmDisplayFrequency = (uint)set.Frequency;

            DisplayChangeResult result = (DisplayChangeResult)SafeNativeMethods.ChangeDisplaySettings(ref mode, 0);

            string msg = null;
            switch (result)
            {
                case DisplayChangeResult.BadDualView:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_BadDualView;
                    break;
                case DisplayChangeResult.BadParam:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_BadParam;
                    break;
                case DisplayChangeResult.BadFlags:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_BadFlags;
                    break;
                case DisplayChangeResult.NotUpdated:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_NotUpdated;
                    break;
                case DisplayChangeResult.BadMode:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_BadMode;
                    break;
                case DisplayChangeResult.Failed:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_Failed;
                    break;
                case DisplayChangeResult.Restart:
                    msg = Properties.Resources.InvalidOperation_Disp_Change_Restart;
                    break;
            }

            if (msg != null)
                throw new InvalidOperationException(msg);
        }

        /// <summary>
        /// Enumerates all supported display modes.
        /// </summary>
        /// <remarks>
        /// Internally calls EnumDisplaySettings() native function.
        /// Because of the nature of EnumDisplaySettings() it calls it many times and uses the <code>yield return</code> statement to simulate an enumerator.
        /// </remarks>
        public static IEnumerator<DisplaySettings> GetModesEnumerator()
        {
            SafeNativeMethods.DEVMODE mode = new SafeNativeMethods.DEVMODE();

            mode.Initialize();

            int idx = 0;

            while (SafeNativeMethods.EnumDisplaySettings(null, idx, ref mode))
                yield return CreateDisplaySettingsObject(idx++, mode);
        }

        /// <summary>
        /// Rotates the screen from its current location by 90 degrees either clockwise or anti-clockwise.
        /// </summary>
        /// <param name="clockwise">Set to true to rotate the screen 90 degrees clockwise from its current location, or false to rotate it anti-clockwise.</param>
        public static void RotateScreen(bool clockwise)
        {
            DisplaySettings set = DisplayManager.GetCurrentSettings();

            int tmp = set.Height;
            set.Height = set.Width;
            set.Width = tmp;

            if (clockwise)
                set.Orientation++;
            else
                set.Orientation--;

            if (set.Orientation < Orientation.Default)
                set.Orientation = Orientation.Clockwise270;
            else if (set.Orientation > Orientation.Clockwise270)
                set.Orientation = Orientation.Default;

            SetDisplaySettings(set);
        }


        /// <summary>
        /// A private helper methods used to derive a DisplaySettings object from the DEVMODE structure.
        /// </summary>
        /// <param name="idx">The mode index attached with the settings. Starts form zero. Is -1 for the current settings.</param>
        /// <param name="mode">The current DEVMODE object represents the display information to derive the DisplaySettings object from.</param>
        private static DisplaySettings CreateDisplaySettingsObject(int idx, SafeNativeMethods.DEVMODE mode)
        {
            return new DisplaySettings()
            {
                Index = idx,
                Width = (int)mode.dmPelsWidth,
                Height = (int)mode.dmPelsHeight,
                Orientation = (Orientation)mode.dmDisplayOrientation,
                BitCount = (int)mode.dmBitsPerPel,
                Frequency = (int)mode.dmDisplayFrequency
            };
        }

        /// <summary>
        /// A private helper method used to retrieve current display settings as a DEVMODE object.
        /// </summary>
        /// <remarks>
        /// Internally calls EnumDisplaySettings() native function with the value ENUM_CURRENT_SETTINGS (-1) to retrieve the current settings.
        /// </remarks>
        private static SafeNativeMethods.DEVMODE GetDeviceMode()
        {
            SafeNativeMethods.DEVMODE mode = new SafeNativeMethods.DEVMODE();

            mode.Initialize();

            if (SafeNativeMethods.EnumDisplaySettings(null, SafeNativeMethods.ENUM_CURRENT_SETTINGS, ref mode))
                return mode;
            else
                throw new InvalidOperationException(GetLastError());
        }

        private static string GetLastError()
        {
            int err = Marshal.GetLastWin32Error();

            string msg;

            if (SafeNativeMethods.FormatMessage(SafeNativeMethods.FORMAT_MESSAGE_FLAGS, SafeNativeMethods.FORMAT_MESSAGE_FROM_HMODULE, (uint)err, 0, out msg, 0, 0) == 0)
                return Properties.Resources.InvalidOperation_FatalError;
            else
                return msg;
        }
    }

    enum Orientation
    {
        Default = 0,
        Clockwise90 = 1,
        Clockwise180 = 2,
        Clockwise270 = 3
    }

    struct DisplaySettings
    {
        public int Index { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Orientation Orientation { get; set; }
        public int BitCount { get; set; }
        public int Frequency { get; set; }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture,
                "{0} by {1}, {2}, {3} Bit, {4} Hertz",
                Width, Height, (int)Orientation, BitCount, Frequency);
        }
    }

    enum DisplayChangeResult
    {
        /// <summary>
        /// Windows XP: The settings change was unsuccessful because system is DualView capable.
        /// </summary>
        BadDualView = -6,
        /// <summary>
        /// An invalid parameter was passed in. This can include an invalid flag or combination of flags.
        /// </summary>
        BadParam = -5,
        /// <summary>
        /// An invalid set of flags was passed in.
        /// </summary>
        BadFlags = -4,
        /// <summary>
        /// Windows NT/2000/XP: Unable to write settings to the registry.
        /// </summary>
        NotUpdated = -3,
        /// <summary>
        /// The graphics mode is not supported.
        /// </summary>
        BadMode = -2,
        /// <summary>
        /// The display driver failed the specified graphics mode.
        /// </summary>
        Failed = -1,
        /// <summary>
        /// The settings change was successful.
        /// </summary>
        Successful = 0,
        /// <summary>
        /// The computer must be restarted in order for the graphics mode to work.
        /// </summary>
        Restart = 1
    }
    
}
