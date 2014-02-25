namespace Sodium.Examples.MouseMove
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private EventSink<MouseEventArgs> sink;
        private Event<Tuple<string, string>> map;
        private IListener listener;

        public Form1()
        {
            InitializeComponent();
            sink = new EventSink<MouseEventArgs>();
            map = sink.Map(ToTuple);
            listener = map.Listen(DisplayMousePosition);
        }

        private static Tuple<string, string> ToTuple(MouseEventArgs e)
        {
            var ci = CultureInfo.InvariantCulture;
            return new Tuple<string, string>(e.X.ToString(ci), e.Y.ToString(ci));
        }

        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            sink.Send(e);
        }

        private void DisplayMousePosition(Tuple<string, string> t)
        {
            x.Text = t.Item1;
            y.Text = t.Item2;
        }

        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            listener.Stop();
            listener = null;
            sink.Stop();
            sink = null;
            map.Stop();
            map = null;
        }
    }
}
