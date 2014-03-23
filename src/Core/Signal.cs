namespace Potassium.Core
{
    using System;
    using System.Threading;
    using Potassium.Providers;

    public class Signal<T> : EventPublisher<T>
    {
        private TimeSpan interval;
        private Timer timer;
        private IProvider<T> source;
        private bool running;

        public Signal(IProvider<T> source)
            : this(source, TimeSpan.MaxValue)
        {
        }
        
        public Signal(IProvider<T> source, TimeSpan interval)
        {
            this.source = source;
            this.interval = interval;
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

        public TimeSpan Interval
        {
            get  {  return interval;  }
            set  {  interval = value; }
        }

        public bool Start()
        {
            if (this.running)
            {
                return false;
            }

            this.timer.Change(this.interval, this.interval);
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
            this.Stop();
            this.Start();
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

        private void Tick(object state)
        {
            this.Publish(this.source.Value);
        }
    }
}
