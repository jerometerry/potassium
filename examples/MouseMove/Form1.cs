namespace Sodium.Examples.MouseMove
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private readonly EventSink<MouseEventArgs> sink = new EventSink<MouseEventArgs>();

        public Form1()
        {
            InitializeComponent();

            var ci = CultureInfo.InvariantCulture;
            sink.Map(e => new Tuple<string, string>(e.X.ToString(ci), e.Y.ToString(ci)))
                .Listen(t => { x.Text = t.Item1; y.Text = t.Item1; });
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            sink.Send(e);
        }
    }
}
