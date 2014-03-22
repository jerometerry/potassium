namespace JT.Rx.Net.Examples.MouseMove
{
    
    
    using System;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private EventPublisher<MouseEventArgs> evt;
        private Event<Tuple<string,string>> formattedEvent;

        public Form1()
        {
            InitializeComponent();
            InitializeMouseHandler();
        }

        private void InitializeMouseHandler()
        {
            // Initialize the Sodium Event
            evt = new EventPublisher<MouseEventArgs>();
            formattedEvent = evt.Map(e => new Tuple<string, string>(e.X.ToString(), e.Y.ToString()));
            formattedEvent.Subscribe(t =>
            {
                x.Text = t.Item1;
                y.Text = t.Item2;
            });

            // Register the mouse move event to publish on the Sodium.Event
            MouseMove += (s, e) => evt.Publish(e);
        }
    }
}
