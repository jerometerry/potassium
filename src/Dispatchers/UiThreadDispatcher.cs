namespace Potassium.Dispatchers
{
    using System;
    using System.Windows.Forms;

    public class UiThreadDispatcher : IDispatcher
    {
        Control control;

        public UiThreadDispatcher(Control control)
        {
            this.control = control;
        }

        public void Dispatch(Action action)
        {
            control.Invoke((MethodInvoker)delegate { action(); });
        }
    }
}
