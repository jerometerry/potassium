namespace Potassium.Examples.Signals
{
    using System;
    using System.Windows.Forms;
    using Potassium.Core;
    using Potassium.Providers;
    using Potassium.Utilities;

    public partial class Form1 : Form
    {
        private const double RadsPerDeg = Math.PI / 180.0;

        private Signal<double> degreesSignal;
        private Event<double> radiansSignal;
        private Event<double> sineSignal;
        private Event<double> cosineSignal;
        private Event<long> signalTickCount;
        private EventPublisher<decimal> intervalChanged;
        private EventPublisher<bool> runningEvent;
        private Behavior<decimal> intervalBehavior;
        private Behavior<bool> runningBehavior;

        public Form1()
        {
            InitializeComponent();

            startBtn.Click += (o, s) => runningEvent.Fire(true);
            
            stopBtn.Click += (o, s) => 
            {
                degreesSignal.Stop();
                runningEvent.Fire(false);
            };
            
            frequency.ValueChanged += (o, s) => intervalChanged.Fire(frequency.Value);

            degreesSignal = new Signal<double>(new AutoDouble(0.0, 0.001), Frequency.Hz(0.0), this.CreateDispatcher());
            degreesSignal.Subscribe(SetDegValue);

            signalTickCount = degreesSignal.Snapshot(new AutoLong());
            signalTickCount.Subscribe(SetTickCount);

            radiansSignal = degreesSignal.Map(d => d * RadsPerDeg);
            radiansSignal.Subscribe(SetRadValue);

            sineSignal = radiansSignal.Map(Math.Sin);
            sineSignal.Subscribe(SetSinValue);

            cosineSignal = radiansSignal.Map(Math.Cos);
            cosineSignal.Subscribe(SetCosValue);

            runningEvent = new EventPublisher<bool>();
            runningEvent.Subscribe(r => { degreesSignal.Running = r; });
            
            runningBehavior = runningEvent.Hold(false);
            runningBehavior.Values().Subscribe(EnableControls);

            intervalChanged = new EventPublisher<decimal>();
            intervalBehavior = intervalChanged.Hold(frequency.Value);
            intervalBehavior
                .Values()
                .Map(Frequency.Hz)
                .Subscribe(FrequencyChanged);
        }

        private void FrequencyChanged(Hz frequence)
        {
            degreesSignal.Frequency = frequence;
            if (degreesSignal.Running)
            {
                degreesSignal.Restart();
            }
        }

        private void SetDegValue(double d)
        {
            degValue.Text = string.Format("{0:N3}", d);
        }

        private void SetRadValue(double r)
        {
            radValue.Text = string.Format("{0:N3}", r);
        }

        private void SetSinValue(double r)
        {
            sinValue.Text = string.Format("{0:N3}", r);
        }

        private void SetCosValue(double r)
        {
            cosValue.Text = string.Format("{0:N3}", r);
        }

        private void SetTickCount(long count)
        {
            ticks.Text = string.Format("{0}", count);
        }

        private void EnableControls(bool running)
        {
            startBtn.Enabled = !running;
            stopBtn.Enabled = running;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            degreesSignal.Stop();
            degreesSignal.Dispose();
            degreesSignal = null;

            radiansSignal.Dispose();
            radiansSignal = null;

            sineSignal.Dispose();
            sineSignal = null;

            signalTickCount.Dispose();
            signalTickCount = null;

            intervalChanged.Dispose();
            intervalChanged = null;

            runningEvent.Dispose();
            runningEvent = null;

            intervalBehavior.Dispose();
            intervalBehavior = null;

            runningBehavior.Dispose();
            runningBehavior = null;
        }
    }
}
