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

        private IEvent<MouseEventArgs> mouseMoveEvent;
        private IEvent<MouseEventArgs> mouseDragEvent;
        private IEvent<MouseStatus> mouseButtonEvent;
        private IBehavior<MouseStatus> mouseButtonBehavior;
        private IBehavior<bool> mouseButtonDownBehavior;
        private static readonly CultureInfo ci = CultureInfo.InvariantCulture;

        public Form1()
        {
            InitializeComponent();
            InitializeMouseHandler();
        }

        private void InitializeMouseHandler()
        {
            this.mouseButtonEvent = new Event<MouseStatus>();
            this.mouseMoveEvent = new Event<MouseEventArgs>();
            this.mouseButtonBehavior = this.mouseButtonEvent.Hold(MouseStatus.Up);
            this.mouseButtonDownBehavior = this.mouseButtonBehavior.MapB(s => s == MouseStatus.Down);
            this.mouseDragEvent = this.mouseMoveEvent.Gate(this.mouseButtonDownBehavior);

            this.mouseButtonBehavior.SubscribeValues(a =>
            {
                this.status.Text = string.Format("{0} {1}", a, DateTime.Now);
            });

            this.mouseDragEvent.Map(e => new Tuple<string, string>(e.X.ToString(ci), e.Y.ToString(ci))).Subscribe(t =>
            {
                x.Text = t.Item1;
                y.Text = t.Item2;
            });

            MouseMove += (s, e) => this.mouseMoveEvent.Fire(e);
            MouseDown += (s, e) => this.mouseButtonEvent.Fire(MouseStatus.Down);
            MouseUp += (s, e) => this.mouseButtonEvent.Fire(MouseStatus.Up);
        }
    }
}
