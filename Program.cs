using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace AltTabHider
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // ── 自动 UAC ────────────────────────────────
            if (!IsRunAsAdmin())
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = Process.GetCurrentProcess().MainModule!.FileName,
                        UseShellExecute = true,
                        Verb = "runas"          // 触发 UAC
                    };
                    Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show("需要管理员权限才能正常工作。",
                                    "AltTabHider", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;  // 退出当前非管理员实例
            }
            // ────────────────────────────────────────────

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>当前进程是否已拥有管理员权限</summary>
        private static bool IsRunAsAdmin()
        {
            using var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
