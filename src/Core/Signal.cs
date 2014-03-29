namespace Potassium.Core
{
    using System;
    using System.Threading;
    using Potassium.Dispatchers;
    using Potassium.Providers;
    using Potassium.Utilities;

    /// <summary>
    /// Signal is an FirableEvent that fires the value of an IProvider using a timer
    /// </summary>
    /// <typeparam name="T">The type of values fired from the Signal</typeparam>
    public class Signal<T> : FirableEvent<T>
    {
        private Hz frequency;
        private Timer timer;
        private IProvider<T> source;
        private bool running;
        private IDispatcher dispatcher;

        /// <summary>
        /// Creates a new disabled Signal
        /// </summary>
        /// <param name="source">The source IProvider who's value will be fired when the Signals timer ticks</param>
        public Signal(IProvider<T> source)
            : this(source, new Hz(0.0))
        {
        }

        /// <summary>
        /// Creates a new Signal
        /// </summary>
        /// <param name="source">The source IProvider who's value will be fired when the Signals timer ticks</param>
        /// <param name="frequency">The frequency of the Signal</param>
        public Signal(IProvider<T> source, Hz frequency)
            : this(source, frequency, new CurrentThreadDispatcher())
        {
        }

        /// <summary>
        /// Creates a new Signal
        /// </summary>
        /// <param name="source">The source IProvider who's value will be fired when the Signals timer ticks</param>
        /// <param name="frequency">The frequency of the Signal</param>
        /// <param name="dispatcher">The dispatcher used to invoke the Signal on the appropriate thread</param>
        public Signal(IProvider<T> source, Hz frequency, IDispatcher dispatcher)
        {
            this.source = source;
            this.frequency = frequency;
            timer = new Timer(Tick);
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Gets / sets the running state of the signal timer
        /// </summary>
        public bool Running
        {
            get 
            { 
                return running; 
            }

            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }

                running = value;
            }
        }

        /// <summary>
        /// Gets / sets the frequency of the signal
        /// </summary>
        public Hz Frequency
        {
            get { return frequency;  }
            set { frequency = value; }
        }

        /// <summary>
        /// Start the signals timer
        /// </summary>
        /// <returns>True if the timer was started, false if the timer was already running</returns>
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

        /// <summary>
        /// Stop the signal timer
        /// </summary>
        /// <returns>True if the timer was stopped, false if the timer was already stopped</returns>
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

        /// <summary>
        /// Restart the timer, if running, using the updated frequency
        /// </summary>
        public void Restart()
        {
            if (this.Running)
            {
                this.SetTimerInterval();
            }
        }

        /// <summary>
        /// Cleanup the current Observable, disposing of any subscriptions.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }

            this.source = null;
            this.dispatcher = null;

            base.Dispose(disposing);
        }

        private void SetTimerInterval()
        {
            if (Math.Abs(this.frequency.Value) <= 1E-15)
            {
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            { 
                var ms = this.frequency.Milliseconds;
                this.timer.Change(ms, ms);
            }
        }

        private void FireCurrentValue()
        {
            this.Fire(this.source.Value);
        }

        private void Tick(object state)
        {
            dispatcher.Dispatch(FireCurrentValue);
        }
    }
}
