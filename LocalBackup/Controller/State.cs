using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalBackup.Controller
{
    public enum State
    {
        Idle,
        FindingChanges,
        ReviewingChanges,
        PerformingChanges,
        Done,
        Canceling,
    }
}
