namespace JT.Rx.Net.Continuous
{
    using JT.Rx.Net.Core;
    using JT.Rx.Net.Discrete;
    using System;
    using System.Threading;

    /// <summary>
    /// ContinuousBehaviorDiscretizer converts a ContinuousBehavior into a discrete stream of Events,
    /// by polling the Behavior.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the Behavior</typeparam>
    public class ContinuousBehaviorDiscretizer<T> : EventPublisher<T>
    {
        private IBehavior<T> behavior;
        private TimeSpan interval;
        private Timer timer;
        private PredicateBehavior until;
        private bool complete;
        private bool running;

        public ContinuousBehaviorDiscretizer(IBehavior<T> behavior, TimeSpan interval, Func<bool> predicate)
            : this(behavior, interval, new QueryPredicateBehavior(predicate))
        {
        }

        public ContinuousBehaviorDiscretizer(IBehavior<T> behavior, TimeSpan interval, PredicateBehavior until)
        {
            this.behavior = behavior;
            this.interval = interval;
            this.until = until;
            this.timer = new Timer(o => this.Publish(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool Running
        {
            get
            {
                return this.running;
            }
        }

        public bool Complete
        {
            get
            {
                return this.complete;
            }
        }

        public bool Valid
        {
            get
            {
                return !until.Value;
            }
        }

        public bool Start()
        {
            if (this.Running)
            {
                return false;
            }

            if (!this.Valid)
            {
                throw new InvalidOperationException("Until has been reached");
            }

            this.complete = false;

            this.timer.Change(this.interval, this.interval);
            return true;
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.running = false;
        }

        private void Publish()
        {
            if (!this.Valid)
            {
                this.complete = true;
                this.Stop();
            }
            else
            {
                this.Publish(behavior.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null)
                {
                    this.Stop();
                    this.timer.Dispose();
                    this.timer = null;
                }

                this.behavior = null;
                this.until = null;
            }

            base.Dispose(disposing);
        }
    }
}
