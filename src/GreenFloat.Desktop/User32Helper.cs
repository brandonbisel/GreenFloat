using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GreenFloat.Desktop
{
    public class User32Helper
    {
        private const uint TPM_LEFTBUTTON = 0X0000;
        private const uint TPM_RETURNCMD = 0X0100;
        private const uint WM_SYSCOMMAND = 0X0112;

        private const uint MF_SEPARATOR = 0x800;
        private const uint MF_BYPOSITION = 0x400;
        private const uint MF_BYCOMMAND = 0x0;
        private const uint MF_STRING = 0x0;

        private const uint MF_ENABLED = 0X0;
        private const uint MF_DISABLED = 0X2;
        private const uint MF_GRAYED = 0X1;

        private const uint SC_CLOSE = 0xF060;
        private const uint SC_MAXIMIZE = 0xF030;
        private const uint SC_MINIMIZE = 0xF020;
        private const uint SC_RESTORE = 0xF120;
        private const uint SC_SIZE = 0xF000;

        private double _aspectRatio;
        private bool? _adjustingHeight = null;

        internal enum SWP
        {
            NOMOVE = 0x0002
        }
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
            EXITSIZEMOVE = 0x0232,
        }

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hWnd,
            IntPtr lpTpm);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DrawMenuBar(IntPtr hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetCursorPos(ref Win32Point lpMousePoint);
        



        private HwndSource HwndSource { get; }

        public User32Helper(Visual visual)
        {
            if (visual == null) throw new ArgumentNullException(nameof(visual));
            this.HwndSource = (HwndSource)PresentationSource.FromVisual(visual);
        }

        public static Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public void SetMinimizeMenuItem(bool enabled)
        {
            if (HwndSource == null)
            {
                return;
            }

            var hMenu = GetSystemMenu(HwndSource.Handle, false);

            var uEnable = enabled ? MF_ENABLED : MF_DISABLED;

            EnableMenuItem(hMenu, MF_BYCOMMAND | SC_MINIMIZE, uEnable);
        }

        public void SetMaximizeMenuItem(bool enabled)
        {
            if (HwndSource == null)
            {
                return;
            }

            var hMenu = GetSystemMenu(HwndSource.Handle, false);

            var uEnable = enabled ? MF_ENABLED : MF_DISABLED;

            EnableMenuItem(hMenu, MF_BYCOMMAND | SC_MAXIMIZE, uEnable);
        }

        public void SetResizeMenuItem(bool enabled)
        {
            if (HwndSource == null)
            {
                return;
            }

            var hMenu = GetSystemMenu(HwndSource.Handle, false);

            var uEnable = enabled ? MF_ENABLED : MF_DISABLED;

            EnableMenuItem(hMenu, MF_BYCOMMAND | SC_SIZE, uEnable);
        }

        public void SetRestoreMenuItem(bool enabled)
        {
            if (HwndSource == null)
            {
                return;
            }

            var hMenu = GetSystemMenu(HwndSource.Handle, false);

            var uEnable = enabled ? MF_ENABLED : MF_DISABLED;

            EnableMenuItem(hMenu, MF_BYCOMMAND | SC_RESTORE, uEnable);
        }

        public void ShowContextMenu(Point point)
        {
            IntPtr wMenu = GetSystemMenu(HwndSource.Handle, false);
            // Display the menu
            uint command = TrackPopupMenuEx(wMenu,
                TPM_LEFTBUTTON | TPM_RETURNCMD, (int)point.X, (int)point.Y, HwndSource.Handle, IntPtr.Zero);
            if (command == 0)
                return;

            SendMessage(HwndSource.Handle, WM_SYSCOMMAND, new IntPtr(command), IntPtr.Zero);
        }

        public void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(HwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        public void LockAspectRatio(double aspectRatio)
        {
            HwndSource.AddHook(DragHook);
            _aspectRatio = aspectRatio;
        }


        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                        if ((pos.flags & (int)SWP.NOMOVE) != 0)
                            return IntPtr.Zero;

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;

                        // determine what dimension is changed by detecting the mouse position relative to the 
                        // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            Point p = GetMousePosition();

                            double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

                            _adjustingHeight = diffHeight > diffWidth;
                        }

                        if (_adjustingHeight.Value)
                            pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }
    }
}