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
        EventPublisher<bool> runningEvent;
        Behavior<decimal> intervalBehavior;
        Behavior<bool> runningBehavior;

        public Form1()
        {
            InitializeComponent();

            startBtn.Click += (o, s) => runningEvent.Publish(true);
            
            stopBtn.Click += (o, s) => 
            {
                signal.Stop();
                runningEvent.Publish(false);
            };
            
            frequency.ValueChanged += (o, s) => intervalChanged.Publish(frequency.Value);

            signal = new Signal<DateTime>(new LocalTime());
            runningEvent = new EventPublisher<bool>();
            intervalChanged = new EventPublisher<decimal>();

            var frm = this;
            signal.Subscribe(t => { frm.RunOnUI<DateTime>(SetDate, t); });
            
            runningEvent.Subscribe(r => { signal.Running = r; });
            runningBehavior = runningEvent.Hold(false);
            runningBehavior.Values().Subscribe(EnableControls);

            intervalBehavior = intervalChanged.Hold(frequency.Value);
            intervalBehavior
                .Values()
                .Map(t => Frequency.Hz(t))
                .Subscribe(FrequencyChanged);
        }

        private void FrequencyChanged(Hz frequence)
        {
            signal.Frequency = frequence;
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
