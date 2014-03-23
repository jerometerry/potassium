namespace Potassium.Examples.Signals
{
    using System;
    using System.Windows.Forms;
    using Potassium.Core;
    using Potassium.Providers;

    public partial class Form1 : Form
    {
        Signal<DateTime> signal;
        EventPublisher<decimal> intervalChanged;
        Behavior<decimal> intervalBehavior;

        EventPublisher<bool> runningEvent;

        Behavior<bool> runningBehavior;

        public Form1()
        {
            InitializeComponent();

            startBtn.Click += (o, s) => runningEvent.Publish(true);
            stopBtn.Click += (o, s) => runningEvent.Publish(false);
            interval.ValueChanged += (o, s) => IntervalChanged();
            
            var frm = this;

            signal = new Signal<DateTime>(new LocalTime());
            signal.Subscribe(t => { frm.RunOnUI<DateTime>(SetDate, t); });

            runningEvent = new EventPublisher<bool>();
            runningEvent.Subscribe(r => { signal.Running = r; });
            runningBehavior = runningEvent.Hold(false);
            runningBehavior.Values().Subscribe(EnableControls);

            intervalChanged = new EventPublisher<decimal>();
            
            intervalBehavior = intervalChanged.Hold(interval.Value);
            intervalBehavior
                .Values()
                .Map(t => TimeSpan.FromMilliseconds((double)t))
                .Subscribe(IntervalChanged);
        }

        private void IntervalChanged()
        {
            intervalChanged.Publish(interval.Value);
        }

        private void IntervalChanged(TimeSpan t)
        {
            signal.Interval = t;
            if (signal.Running)
            {
                signal.Restart();
            }
        }

        private void SetDate(DateTime t)
        {
            latestValue.Text = string.Format("{0:dd/MM/yy HH:mm:ss.fff}", t);
        }

        private void EnableControls(bool running)
        {
            this.startBtn.Enabled = !running;
            this.stopBtn.Enabled = running;
        }
    }
}
