using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Potassium.Examples.Signals
{
    public static class Extensions
    {
        public static void RunOnUI<T>(this Form control, Action<T> action, T value)
        {
            if (!control.InvokeRequired)
            {
                action(value);
                return;
            }

            control.Invoke((MethodInvoker)delegate { action(value); });
        }
    }
}
