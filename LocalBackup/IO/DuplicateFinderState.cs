using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalBackup.IO
{
    public enum DuplicateFinderState
    {
        Idle,
        FindingFiles,
        SortingFiles,
        FindingDuplicates
    }
}
