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
            // ���� �Զ� UAC ����������������������������������������������������������������
            if (!IsRunAsAdmin())
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = Process.GetCurrentProcess().MainModule!.FileName,
                        UseShellExecute = true,
                        Verb = "runas"          // ���� UAC
                    };
                    Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show("��Ҫ����ԱȨ�޲�������������",
                                    "AltTabHider", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;  // �˳���ǰ�ǹ���Աʵ��
            }
            // ����������������������������������������������������������������������������������������

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>��ǰ�����Ƿ���ӵ�й���ԱȨ��</summary>
        private static bool IsRunAsAdmin()
        {
            using var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
