namespace Potassium.Examples.MouseDrag
{
    using System;
    using System.Windows.Forms;
    using Potassium.Core;
    using Potassium.Extensions;
    using Potassium.Providers;

    public partial class Form1 : Form
    {
        private EventPublisher<MouseEventArgs> mouseMoveEvent;
        private Event<MouseEventArgs> mouseDragEvent;
        private EventPublisher<MouseStatus> mouseButtonEvent;
        private Behavior<MouseStatus> mouseButtonBehavior;
        private Event<MouseStatus> mouseButtonUpdates;
        private Predicate mouseButtonDown;

        public Form1()
        {
            InitializeComponent();
            InitializeMouseHandler();
        }

        private void InitializeMouseHandler()
        {
            mouseButtonEvent = new EventPublisher<MouseStatus>();
            mouseMoveEvent = new EventPublisher<MouseEventArgs>();
            mouseButtonBehavior = mouseButtonEvent.Hold(MouseStatus.Up);
            this.mouseButtonDown = new EqualityPredicate<MouseStatus>(mouseButtonBehavior,  MouseStatus.Down);
            mouseDragEvent = mouseMoveEvent.Gate(this.mouseButtonDown);
            mouseButtonUpdates = mouseButtonBehavior.Values();
            mouseButtonUpdates.Subscribe(a =>
            {
                status.Text = string.Format("{0} {1}", a, DateTime.Now);
            });

            mouseDragEvent.Map(e => new Tuple<string, string>(e.X.ToString(), e.Y.ToString())).Subscribe(t =>
            {
                x.Text = t.Item1;
                y.Text = t.Item2;
            });

            MouseMove += (s, e) => mouseMoveEvent.Fire(e);
            MouseDown += (s, e) => mouseButtonEvent.Fire(MouseStatus.Down);
            MouseUp += (s, e) => mouseButtonEvent.Fire(MouseStatus.Up);
        }
    }
}
