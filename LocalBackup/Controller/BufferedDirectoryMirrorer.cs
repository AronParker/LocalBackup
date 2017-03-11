using System;
using System.Collections.Generic;
using LocalBackup.IO;
using LocalBackup.IO.Operations;

namespace LocalBackup.Controller
{
    public class BufferedDirectoryMirrorer : DirectoryMirrorer
    {
        private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;

        public event EventHandler QueueFlushRequested;

        public Queue<object> ProcessingQueue { get; } = new Queue<object>();

        protected override void OnOperationFound(FileSystemOperation operation)
        {
            EnqueueItem(operation);
        }

        protected override void OnError(Exception ex)
        {
            EnqueueItem(ex);
        }

        private void EnqueueItem(object item)
        {
            ProcessingQueue.Enqueue(item);

            var now = DateTimeOffset.UtcNow;

            if ((now - _lastUpdate).TotalMilliseconds >= 500)
            {
                QueueFlushRequested?.Invoke(this, EventArgs.Empty);

                _lastUpdate = now;
            }
        }
    }
}
