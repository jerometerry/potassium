namespace Sodium.Examples.MouseMove
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private EventPublisher<MouseEventArgs> evt;

        public Form1()
        {
            InitializeComponent();
            InitializeMouseHandler();
        }

        private static Tuple<string, string> Format(MouseEventArgs e)
        {
            var ci = CultureInfo.InvariantCulture;
            return new Tuple<string, string>(e.X.ToString(ci), e.Y.ToString(ci));
        }

        private void InitializeMouseHandler()
        {
            // Initialize the Sodium Event
            evt = new EventPublisher<MouseEventArgs>();
            evt.Map(Format).Subscribe(DisplayMousePosition);

            // Register the mouse move event to publish on the Sodium.Event
            MouseMove += (s, e) => evt.Publish(e);
        }

        private void DisplayMousePosition(Tuple<string, string> t)
        {
            x.Text = t.Item1;
            y.Text = t.Item2;
        }
    }
}
