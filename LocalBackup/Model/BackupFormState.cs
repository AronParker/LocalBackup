namespace LocalBackup.Model
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
