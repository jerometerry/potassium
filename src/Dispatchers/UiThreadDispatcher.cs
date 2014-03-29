namespace Potassium.Dispatchers
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// UiThreadDispatcher is an IDispatcher that dispatches methods on the UI thread
    /// </summary>
    public class UiThreadDispatcher : IDispatcher
    {
        private readonly Control control;

        /// <summary>
        /// Creates a new UiThreadDispatcher
        /// </summary>
        /// <param name="control">The control used to dispatch to the UI thread.</param>
        public UiThreadDispatcher(Control control)
        {
            this.control = control;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Dispatch(Action action)
        {
            if (!control.IsDisposed && !control.Disposing)
            { 
                SafeInvoke(() => control.Invoke((MethodInvoker)(() => action())));
            }
        }

        private void SafeInvoke(Action action)
        {
            try
            {
                if (!control.IsDisposed && !control.Disposing)
                { 
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
