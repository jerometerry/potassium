namespace Sodium.Examples.MouseDrag
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private enum MouseStatus
        {
            Down,
            Up
        }

        private EventPublisher<MouseEventArgs> mouseMoveEvent;
        private Event<MouseEventArgs> mouseDragEvent;
        private EventPublisher<MouseStatus> mouseButtonEvent;
        private EventBasedBehavior<MouseStatus> mouseButtonBehavior;
        private Event<MouseStatus> mouseButtonUpdates;
        private PredicateBehavior mouseButtonDownBehavior;
        private static readonly CultureInfo ci = CultureInfo.InvariantCulture;

        public Form1()
        {
            InitializeComponent();
            InitializeMouseHandler();
        }

        private void InitializeMouseHandler()
        {
            this.mouseButtonEvent = new EventPublisher<MouseStatus>();
            this.mouseMoveEvent = new EventPublisher<MouseEventArgs>();
            this.mouseButtonBehavior = this.mouseButtonEvent.Hold(MouseStatus.Up);
            this.mouseButtonDownBehavior = new EqualityPredicateBehavior<MouseStatus>(mouseButtonBehavior,  MouseStatus.Down);
            this.mouseDragEvent = this.mouseMoveEvent.Gate(this.mouseButtonDownBehavior);
            this.mouseButtonUpdates = this.mouseButtonBehavior.Values();
            this.mouseButtonUpdates.Subscribe(a =>
            {
                this.status.Text = string.Format("{0} {1}", a, DateTime.Now);
            });

            this.mouseDragEvent.Map(e => new Tuple<string, string>(e.X.ToString(ci), e.Y.ToString(ci))).Subscribe(t =>
            {
                x.Text = t.Item1;
                y.Text = t.Item2;
            });

            MouseMove += (s, e) => this.mouseMoveEvent.Publish(e);
            MouseDown += (s, e) => this.mouseButtonEvent.Publish(MouseStatus.Down);
            MouseUp += (s, e) => this.mouseButtonEvent.Publish(MouseStatus.Up);
        }
    }
}
