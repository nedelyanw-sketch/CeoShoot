using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CeoShootMain
{
    public class BackgroundControllerForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const int HotkeyId = 1;
        private const uint ModNone = 0x0000;
        private const uint VkSnapshot = 0x2C;

        private NotifyIcon _trayIcon;
        private Icon _appIcon;

        public BackgroundControllerForm()
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.Size = new Size(0, 0);

            _appIcon = CreateCeoShootIcon();
            this.Icon = _appIcon;

            InitTrayIcon();
            RegisterHotKey(this.Handle, HotkeyId, ModNone, VkSnapshot);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000080;
                return cp;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Visible = false;
        }

        private void InitTrayIcon()
        {
            ContextMenu strip = new ContextMenu();
            strip.MenuItems.Add("Take Screenshot", (s, e) => TriggerScreenshot());
            strip.MenuItems.Add("-");
            strip.MenuItems.Add("Exit", (s, e) =>
            {
                _trayIcon.Visible = false;
                Application.Exit();
            });

            _trayIcon = new NotifyIcon
            {
                Icon = _appIcon,
                ContextMenu = strip,
                Text = "CEOSHOOT",
                Visible = true
            };

            _trayIcon.Click += (s, e) =>
            {
                if (((MouseEventArgs)e).Button == MouseButtons.Left) TriggerScreenshot();
            };
        }

        private Icon CreateCeoShootIcon()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(256, 256))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    Rectangle bgRect = new Rectangle(10, 10, 236, 236);
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 55;
                        path.AddArc(bgRect.X, bgRect.Y, radius * 2, radius * 2, 180, 90);
                        path.AddArc(bgRect.Right - radius * 2, bgRect.Y, radius * 2, radius * 2, 270, 90);
                        path.AddArc(bgRect.Right - radius * 2, bgRect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        path.AddArc(bgRect.X, bgRect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        path.CloseAllFigures();

                        using (SolidBrush whiteBrush = new SolidBrush(Color.FromArgb(248, 249, 250)))
                        {
                            g.FillPath(whiteBrush, path);
                        }
                    }

                    Rectangle lensRect = new Rectangle(48, 48, 160, 160);
                    using (LinearGradientBrush lensBrush = new LinearGradientBrush(
                        lensRect,
                        Color.FromArgb(222, 47, 122),
                        Color.FromArgb(114, 61, 222),
                        45f))
                    {
                        g.FillEllipse(lensBrush, lensRect);
                    }

                    using (Pen linePen = new Pen(Color.FromArgb(248, 249, 250), 3f))
                    {
                        g.DrawLine(linePen, 128, 48, 90, 114);
                        g.DrawLine(linePen, 90, 114, 110, 196);
                        g.DrawLine(linePen, 110, 196, 175, 175);
                        g.DrawLine(linePen, 175, 175, 190, 100);
                        g.DrawLine(linePen, 190, 100, 128, 48);

                        g.DrawLine(linePen, 90, 114, 48, 140);
                        g.DrawLine(linePen, 110, 196, 145, 208);
                        g.DrawLine(linePen, 175, 175, 205, 140);
                        g.DrawLine(linePen, 190, 100, 160, 52);
                        g.DrawLine(linePen, 128, 48, 85, 62);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    Icon icon = Icon.FromHandle(hIcon).Clone() as Icon;

                    DestroyIcon(hIcon);
                    return icon;
                }
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WmHotkey = 0x0312;
            if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId) TriggerScreenshot();
            base.WndProc(ref m);
        }

        private void TriggerScreenshot()
        {
            if (Application.OpenForms["SelectionForm"] != null) return;

            Bitmap backgroundScreen = CaptureScreen();
            using (SelectionForm selForm = new SelectionForm(backgroundScreen))
            {
                selForm.Icon = _appIcon;
                selForm.ShowDialog();
            }
        }

        private Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            UnregisterHotKey(this.Handle, HotkeyId);
            _trayIcon.Dispose();
            _appIcon?.Dispose();
            base.OnClosing(e);
        }
    }
}