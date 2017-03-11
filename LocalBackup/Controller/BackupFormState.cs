namespace LocalBackup.Controller
{
    public enum BackupFormState
    {
        Idle,
        FindingChanges,
        ReviewingChanges,
        PerformingChanges,
        Done,
        Canceling,
    }
}
