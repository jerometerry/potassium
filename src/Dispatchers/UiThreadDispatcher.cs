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

        public void Dispatch(Action action)
        {
            control.Invoke((MethodInvoker)(() => action()));
        }
    }
}
