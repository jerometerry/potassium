namespace Potassium.Examples.Signals
{
    using System.Windows.Forms;
    using Potassium.Dispatchers;

    public static class Extensions
    {
        public static IDispatcher CreateDispatcher(this Form control)
        {
            return new UiThreadDispatcher(control);
        }
    }
}
