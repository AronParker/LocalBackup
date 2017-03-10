using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocalBackup.Forms;

namespace LocalBackup.Controller
{
    public class BackupController
    {
        private MainForm _mainForm;
        private State _state;

        public BackupController()
        {
            _mainForm = new MainForm();
        }
        
        public void Run()
        {
            Application.Run(_mainForm);
        }
    }
}
