using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AltTabHider
{
    public class MainForm : Form
    {
        private readonly ListView _lv;
        private readonly Button _btnHide, _btnShowOnly, _btnMin, _btnRestore, _btnRefresh;

        private readonly Dictionary<IntPtr, int> _origStyle = new();
        private readonly Dictionary<IntPtr, NotifyIcon> _windowTray = new();
        private readonly NotifyIcon _appTray;

        public MainForm()
        {
            /* —— 全局放大字体 —— */
            Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 11f, FontStyle.Regular);

            Text = "Alt+Tab Hider";
            Width = 960;
            Height = 600;

            /* —— 列表（强制 32 px 行高 via 透明 ImageList） —— */
            _lv = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Top,
                Height = ClientSize.Height - 110
            };
            _lv.Columns.Add("Handle", 110, HorizontalAlignment.Left);
            _lv.Columns.Add("Title", 360, HorizontalAlignment.Left);
            _lv.Columns.Add("Process", 190, HorizontalAlignment.Left);
            _lv.Columns.Add("PID", 80, HorizontalAlignment.Left);

            var img = new ImageList { ImageSize = new Size(32, 32), TransparentColor = Color.Transparent };
            img.Images.Add(new Bitmap(32, 32));             // index 0: 透明占位
            _lv.SmallImageList = img;                       // 行高≈ImageSize.Y

            Controls.Add(_lv);

            /* —— 按钮：FlowLayout 自动适配 DPI —— */
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(8, 8, 8, 0)
            };
            _btnHide = MakeBtn("Hide", (_, __) => HideSelected());
            _btnShowOnly = MakeBtn("Show (no Alt‑Tab)", (_, __) => ShowOnlySelected());
            _btnMin = MakeBtn("Minimize", (_, __) => MinimizeSelected());
            _btnRestore = MakeBtn("Restore (full)", (_, __) => RestoreSelected());
            _btnRefresh = MakeBtn("Refresh", (_, __) => LoadWindows());
            btnPanel.Controls.AddRange(new Control[] { _btnHide, _btnShowOnly, _btnMin, _btnRestore, _btnRefresh });
            Controls.Add(btnPanel);

            /* —— 主托盘 —— */
            _appTray = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "AltTabHider",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
            _appTray.ContextMenuStrip.Items.Add("Show", null, (_, __) => ShowFromTray());
            _appTray.ContextMenuStrip.Items.Add("Exit", null, (_, __) => ExitFromTray());
            _appTray.DoubleClick += (_, __) => ShowFromTray();

            Load += (_, _) => LoadWindows();
        }

        /* ---------- 工具：生成放大按钮 ---------- */
        private static Button MakeBtn(string txt, EventHandler click)
        {
            var b = new Button
            {
                Text = txt,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(6),
                Margin = new Padding(6)
            };
            b.Click += click;
            return b;
        }

        /* ========== 主窗托盘 & 退出统一 Restore ========== */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                RestoreAllHidden();
                base.OnFormClosing(e);
            }
        }
        private void ShowFromTray()
        {
            Show(); WindowState = FormWindowState.Normal; Activate();
        }
        private void ExitFromTray()
        {
            RestoreAllHidden();
            _appTray.Visible = false;
            foreach (var ni in _windowTray.Values) ni.Dispose();
            Application.Exit();
        }
        private void RestoreAllHidden()
        {
            foreach (var h in new List<IntPtr>(_windowTray.Keys))
                RestoreFull(h);
        }

        /* ---------- 选中行句柄 ---------- */
        private IEnumerable<IntPtr> SelectedHandles()
        {
            foreach (ListViewItem li in _lv.SelectedItems)
                if (li.Tag is IntPtr h && IsWindow(h))
                    yield return h;
        }

        /* ---------- 按钮批量动作 ---------- */
        private void HideSelected() { foreach (var h in SelectedHandles()) HideTarget(h, WindowText(h)); }
        private void ShowOnlySelected() { foreach (var h in SelectedHandles()) ShowOnly(h); }
        private void MinimizeSelected() { foreach (var h in SelectedHandles()) MinimizeWindow(h); }
        private void RestoreSelected() { foreach (var h in SelectedHandles()) RestoreFull(h); }

        /* ================= 单窗逻辑 ================= */
        private void HideTarget(IntPtr hWnd, string title)
        {
            if (_origStyle.ContainsKey(hWnd)) return;

            int cur = GetWindowLong(hWnd, GWL_EXSTYLE);
            _origStyle[hWnd] = cur;
            int newStyle = cur & ~WS_EX_APPWINDOW | WS_EX_TOOLWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, newStyle);
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            var icon = ExtractSmallIcon(hWnd) ?? SystemIcons.Application;
            var ni = new NotifyIcon { Icon = icon, Text = $"[Hidden] {title}", Visible = true };

            var cms = new ContextMenuStrip();
            cms.Items.Add("Show (no Alt‑Tab)", null, (_, __) => ShowOnly(hWnd));
            cms.Items.Add("Minimize", null, (_, __) => MinimizeWindow(hWnd));
            cms.Items.Add("Restore (full)", null, (_, __) => RestoreFull(hWnd));
            ni.ContextMenuStrip = cms;
            ni.DoubleClick += (_, __) => ShowOnly(hWnd);

            _windowTray[hWnd] = ni;
        }

        private void ShowOnly(IntPtr hWnd)
        {
            if (!IsWindow(hWnd)) return;
            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
        }
        private void MinimizeWindow(IntPtr hWnd)
        {
            if (!IsWindow(hWnd)) return;
            ShowWindow(hWnd, SW_MINIMIZE);
        }
        private void RestoreFull(IntPtr hWnd)
        {
            if (!IsWindow(hWnd)) return;

            int style = GetWindowLong(hWnd, GWL_EXSTYLE);
            style &= ~WS_EX_TOOLWINDOW;
            style |= WS_EX_APPWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, style);
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            _origStyle.Remove(hWnd);
            if (_windowTray.TryGetValue(hWnd, out var ni))
            { ni.Visible = false; ni.Dispose(); _windowTray.Remove(hWnd); }

            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
        }

        /* ========== 列表刷新 ========== */
        private void LoadWindows()
        {
            _lv.Items.Clear();
            EnumWindows((hWnd, _) =>
            {
                if (!IsWindowVisible(hWnd)) return true;
                if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero) return true;
                if (GetWindowTextLength(hWnd) == 0) return true;

                uint pid; GetWindowThreadProcessId(hWnd, out pid);
                string title = WindowText(hWnd);

                var it = new ListViewItem(hWnd.ToString("X8"), 0) { Tag = hWnd };
                it.SubItems.Add(title);

                string proc = "N/A";
                try { proc = Process.GetProcessById(unchecked((int)pid)).ProcessName; } catch { }
                it.SubItems.Add(proc);
                it.SubItems.Add(pid.ToString());

                _lv.Items.Add(it);
                return true;
            }, IntPtr.Zero);
        }
        private static string WindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd);
            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /* ========== 工具：取小图标 ========== */
        private static Icon? ExtractSmallIcon(IntPtr hWnd)
        {
            IntPtr hIcon = SendMessage(hWnd, WM_GETICON, 2, 0);
            if (hIcon == IntPtr.Zero) hIcon = GetClassLongPtr(hWnd, GCL_HICONSM);
            return hIcon == IntPtr.Zero ? null : Icon.FromHandle(hIcon);
        }

        /* ========== Win32 ========== */
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int GW_OWNER = 4;

        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;

        private const int WM_GETICON = 0x7F;
        private const int GCL_HICONSM = -34;

        private const int SW_RESTORE = 9;
        private const int SW_MINIMIZE = 6;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                                                int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMax);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmd);

        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex) =>
            IntPtr.Size == 8 ? GetClassLongPtr64(hWnd, nIndex) : new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        [DllImport("user32.dll", EntryPoint = "GetClassLong")] private static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")] private static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);
    }
}
