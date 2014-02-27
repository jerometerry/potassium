namespace Sodium.Examples.MouseMove
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private Event<MouseEventArgs> evt;

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
            evt = new Event<MouseEventArgs>();
            MouseMove += (s, e) => evt.Fire(e);
            evt.Map(Format).Listen(DisplayMousePosition);
        }

        private void DisplayMousePosition(Tuple<string, string> t)
        {
            x.Text = t.Item1;
            y.Text = t.Item2;
        }
    }
}
