using System;
using Avalonia.Threading;

namespace ME3Server_WV
{
    public static class DispatcherHelper
    {
        /// <summary>
        /// Runs the action on the UI thread. If already on the UI thread, runs synchronously;
        /// otherwise posts to the dispatcher.
        /// </summary>
        public static void RunOnUI(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
                action();
            else
                Dispatcher.UIThread.Post(action);
        }
    }
}
