using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LocalBackup.Controls
{
    public class ExplorerListView : ListView
    {
        public ExplorerListView()
        {
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            var hr = NativeMethods.SetWindowTheme(Handle, "explorer", null);

            Debug.Assert(hr == 0);

            base.OnHandleCreated(e);
        }
    }
}
