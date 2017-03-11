using System;
using System.Windows.Forms;
using LocalBackup.Controller;

namespace LocalBackup
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new BackupController().Run();
        }
    }
}
