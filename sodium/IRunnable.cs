using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sodium
{
    public interface IRunnable
    {
        void run();
    }

    public class RunnableImpl : IRunnable
    {
        private Action action;
        public RunnableImpl(Action action)
        {
            this.action = action;
        }

        public void run()
        {
            action();
        }
    }
}
