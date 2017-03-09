﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LocalBackup.Controls
{
    public class ListViewEx : ListView
    {
        public ListViewEx()
        {
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            var hResult = NativeMethods.SetWindowTheme(Handle, "explorer", null);

            Debug.Assert(hResult == 0);

            base.OnHandleCreated(e);
        }
    }
}
