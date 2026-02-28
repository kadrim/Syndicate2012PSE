using System;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;

namespace ME3Server_WV
{
    public partial class GUI_Log : UserControl
    {
        private int _renderedCount = 0;
        private bool _updatePending = false;

        public GUI_Log()
        {
            InitializeComponent();

            Logger.LogEntries.CollectionChanged += LogEntries_CollectionChanged;

            // Render all entries that already exist
            SyncEntries();
        }

        private void LogEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScheduleUpdate();
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                DispatcherHelper.RunOnUI(() =>
                {
                    logTextBlock.Inlines?.Clear();
                    _renderedCount = 0;
                    _updatePending = false;
                });
            }
        }

        private void ScheduleUpdate()
        {
            if (_updatePending) return;
            _updatePending = true;

            // Batch all pending entries into a single UI update on the next dispatcher cycle
            Dispatcher.UIThread.Post(() =>
            {
                _updatePending = false;
                SyncEntries();
                logScrollViewer.ScrollToEnd();
            }, DispatcherPriority.Background);
        }

        private void SyncEntries()
        {
            var entries = Logger.LogEntries;
            for (int i = _renderedCount; i < entries.Count; i++)
                AppendRun(entries[i]);
            _renderedCount = entries.Count;
        }

        private void AppendRun(LogEntry entry)
        {
            if (logTextBlock.Inlines == null) return;
            var brush = LogColorConverter.Instance.Convert(entry.Color, typeof(IBrush), null, null) as IBrush ?? Brushes.White;
            var run = new Run(entry.Message + Environment.NewLine) { Foreground = brush };
            logTextBlock.Inlines.Add(run);
        }
    }
}
