namespace Potassium.Core
{
    using System;
    using System.Threading;
    using Potassium.Providers;

    /// <summary>
    /// Signal is an EventPublisher that publishes the value of an IProvider using a timer
    /// </summary>
    /// <typeparam name="T">The type of values published from the Signal</typeparam>
    public class Signal<T> : EventPublisher<T>
    {
        private Hz frequency;
        private Timer timer;
        private IProvider<T> source;
        private bool running;

        public Signal(IProvider<T> source)
            : this(source, new Hz(0.0))
        {
        }

        public Signal(IProvider<T> source, Hz frequency)
        {
            this.source = source;
            this.frequency = frequency;
            timer = new Timer(Tick);
        }

        public bool Running
        {
            get 
            { 
                return running; 
            }
            set
            {
                if (value)
                    Start();
                else
                    Stop();

                running = value;
            }
        }

        public Hz Frequency
        {
            get  {  return frequency;  }
            set { frequency = value; }
        }

        public bool Start()
        {
            if (this.running)
            {
                return false;
            }

            this.SetTimerInterval();
            
            this.running = true;
            return true;
        }

        public bool Stop()
        {
            if (!this.running)
            {
                return false;
            }

            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.running = false;

            return true;
        }

        public void Restart()
        {
            if (this.Running)
            {
                this.SetTimerInterval();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
                this.timer.Dispose();
                this.timer = null;
                this.source = null;
            }

            base.Dispose(disposing);
        }

        private void SetTimerInterval()
        {
            if (this.frequency.Value == 0.0)
            {
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            { 
                var interval = this.frequency.Interval();
                this.timer.Change(interval, interval);
            }
        }

        private void Tick(object state)
        {
            this.Publish(this.source.Value);
        }
    }
}
