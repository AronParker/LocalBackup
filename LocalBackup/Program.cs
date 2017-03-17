using System;
using System.Windows.Forms;
using LocalBackup.Forms;

namespace LocalBackup
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BackupForm());
        }
    }
}
