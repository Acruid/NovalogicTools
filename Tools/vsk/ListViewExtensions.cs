using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace vsk
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ListViewExtensions
    {
        #region SetSortIcon()

        // Source: http://stackoverflow.com/a/254139

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4 // HDI_FORMAT
            }

            [Flags]
            public enum Format
            {
                SortDown = 0x200, // HDF_SORTDOWN
                SortUp = 0x400 // HDF_SORTUP
            }
        }

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = LVM_FIRST + 31;

        public const int HDM_FIRST = 0x1200;
        public const int HDM_GETITEM = HDM_FIRST + 11;
        public const int HDM_SETITEM = HDM_FIRST + 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref HDITEM lParam);

        public static void SetSortIcon(this ListView listViewControl, int columnIndex, SortOrder order)
        {
            var columnHeader = SendMessage(listViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            for (var columnNumber = 0; columnNumber <= listViewControl.Columns.Count - 1; columnNumber++)
            {
                var columnPtr = new IntPtr(columnNumber);
                var item = new HDITEM
                {
                    mask = HDITEM.Mask.Format
                };

                if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                if (order != SortOrder.None && columnNumber == columnIndex)
                {
                    switch (order)
                    {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDITEM.Format.SortDown;
                            item.fmt |= HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDITEM.Format.SortUp;
                            item.fmt |= HDITEM.Format.SortDown;
                            break;
                    }
                }
                else
                {
                    item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                }

                if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
            }
        }

        #endregion

        #region PreferredWidth()

        //Source: http://www.pinvoke.net/default.aspx/user32/GetScrollInfo.html

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/bb787537(v=vs.85).aspx
        /// </summary>
        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct SCROLLINFO
        {
            public uint cbSize; // size in bytes of this struct
            public uint fMask;
            public readonly int nMin;
            public readonly int nMax;
            public readonly uint nPage;
            public readonly int nPos;
            public readonly int nTrackPos;
        }

        private enum SBOrientation
        {
            SB_HORZ = 0x0,
            SB_VERT = 0x1,
            SB_CTL = 0x2,
            SB_BOTH = 0x3
        }

        private enum ScrollInfoMask : uint
        {
            SIF_RANGE = 0x1,
            SIF_PAGE = 0x2,
            SIF_POS = 0x4,
            SIF_DISABLENOSCROLL = 0x8,
            SIF_TRACKPOS = 0x10,
            SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        /// <summary>
        /// Returns the width of a UserControl such that no horizontal scrollbar will be needed
        /// </summary>
        /// <param name="listView"></param>
        /// <returns></returns>
        public static int PreferredWidth(this ListView listView)
        {
            const int minWidth = 10;
            var borderWidth = SystemInformation.Border3DSize.Width;

            var si = new SCROLLINFO();
            si.cbSize = (uint) Marshal.SizeOf(si);
            si.fMask = (int) ScrollInfoMask.SIF_RANGE;
            GetScrollInfo(listView.Handle, (int) SBOrientation.SB_HORZ, ref si);
            

            // are vertical & horizontal scrolls swapped in C#?
            int scrollBarWidth;
            var scrollBars = GetVisibleScrollbars(listView);
            if (scrollBars == ScrollBars.Both || scrollBars == ScrollBars.Vertical)
                scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            else
                scrollBarWidth = 0;

            var iWidth = si.nMax - si.nMin + 2*borderWidth + scrollBarWidth + 2;
            return Math.Max(minWidth, iWidth);
        }

        #endregion

        #region AutoCalcWidth()

        //Source: http://www.pinvoke.net/default.aspx/user32/GetScrollBarInfo.html

        private enum ScrollObjMask : uint
        {
            OBJID_HSCROLL = 0xFFFFFFFA,
            OBJID_VSCROLL = 0xFFFFFFFB,
            OBJID_CLIENT = 0xFFFFFFFC,
        }

        private enum StateSystem : uint
        {
            STATE_SYSTEM_INVISIBLE = 0x00008000,
            STATE_SYSTEM_OFFSCREEN = 0x00010000,
            STATE_SYSTEM_PRESSED = 0x00000008,
            STATE_SYSTEM_UNAVAILABLE = 0x00000001
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/bb787535(v=vs.85).aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLBARINFO
        {
            public int cbSize;
            public RECT rcScrollBar;
            public int dxyLineButton;
            public int xyThumbTop;
            public int xyThumbBottom;
            public int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] rgstate;
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/bb787581(v=vs.85).aspx
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        private static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);

        public static int AutoCalcWidth(this ListView listView)
        {
            var sbi = new SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (GetScrollBarInfo(listView.Handle, (uint) ScrollObjMask.OBJID_HSCROLL, ref sbi) != 0)
            {
                const int minWidth = 100;
                var borderWidth = SystemInformation.Border3DSize.Width;
                var scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                var scrollBars = GetVisibleScrollbars(listView);

                if (scrollBars == ScrollBars.Both || scrollBars == ScrollBars.Horizontal)
                    scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                else
                {
                    scrollBarWidth = 0;
                }

                int state = sbi.rgstate[0];

                var iWidth = 0;
//                var iWidth = si.nMax - si.nMin + 2 * borderWidth + scrollBarWidth + 2;
                return Math.Max(minWidth, iWidth);
            }
            var error = Marshal.GetLastWin32Error();
            throw new Exception($"Pinvoked GetScrollBarInfo error:{error}");
        }

        #endregion

        #region AutoCalcLong()
        // offset of window style value
        public const int GWL_STYLE = -16;

        // window style constants for scrollbars
        public const int WS_VSCROLL = 0x00200000;
        public const int WS_HSCROLL = 0x00100000;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static ScrollBars GetVisibleScrollbars(ListView listView)
        {
            int wndStyle = GetWindowLong(listView.Handle, GWL_STYLE);
            bool hsVisible = (wndStyle & WS_HSCROLL) != 0;
            bool vsVisible = (wndStyle & WS_VSCROLL) != 0;

            if (hsVisible)
                return vsVisible ? ScrollBars.Both : ScrollBars.Horizontal;
            else
                return vsVisible ? ScrollBars.Vertical : ScrollBars.None;
        }
        #endregion
    }
}