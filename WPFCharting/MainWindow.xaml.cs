using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace WPFCharting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i;
        public static Line xAxisLine, yAxisLine, line;
        public static double xAxisStart = 150, yAxisStart = 250, yAxisStop = 50;
        public static double xinterval { get; set; } = 50;
        public static double yinterval { get; set; } = 50;
        public double yPointInterval;
        public int ysegments = 10;
        public static double ystart = 0, ystop = 50;
        double yPoint, xPoint, yValue, xValue;
        public static double yscale;
        private int count;
        private int colorCount;
        private int textOutLines, metingen = 50;
        private string ReceivedData;
        public string data;
        private string tempS;
        private Polyline chartPolyline;
        private SerialPort SerPort;
        Regex regexN = new Regex("^-{0,1}[0-9]{0,}$");
        Regex regex = new Regex("[^0-9]+");
        TextBlock yTextBlock0, textBlock;
        TextBox box;
        private Point origin;
        public static string[] split = new string[] { };
        String[] ports;
        private List<channel> channels = new List<channel>();
        private SolidColorBrush[] colors = new SolidColorBrush[] {
            Brushes.Blue,
            Brushes.Brown,
            Brushes.Red,
            Brushes.Green, 
            Brushes.Magenta,
            Brushes.Yellow,
            Brushes.DarkGreen
        };

        public MainWindow()
        {
            InitializeComponent();
            this.StateChanged += (sender, e) =>
            {
                run();
            };
            
            this.SizeChanged += (sender, e) =>
            {
                run();
            };
            FetchAvailablePorts();
            connectButton.Click += (sender, e) =>
            {
                connect();
            };
            refreshButton.Click += (sender, e) =>
            {
                FetchAvailablePorts();
            };

            metingenBox.TextChanged += (sender, e) =>
            {
                try
                {
                    metingen = Int32.Parse(metingenBox.Text);
                    foreach (var channel in channels)
                    {
                        channel.Sizing(metingen);
                    }
                    run();
                }
                catch { }
            };

            YMax.TextChanged += (sender, e) =>
            {
                if (scaleOverride.IsChecked == true)
                {
                    try
                    {
                        ystop = Double.Parse(YMax.Text);
                        run();
                    }
                    catch { }
                }
            };

            YMin.TextChanged += (sender, e) =>
            {
                try
                {
                    if (scaleOverride.IsChecked == true)
                    {
                        ystart = Double.Parse(YMin.Text);
                        run();
                    }
                }
                catch { }
            };

            scaleOverride.Checked += (sender, e) =>
            {
                ystop = Double.Parse(YMax.Text);
                ystart = Double.Parse(YMin.Text);
                run();
            };

            ySeg.TextChanged += (sender, e) =>
            {
                try {
                    ysegments = Int32.Parse(ySeg.Text);
                    run();
                } catch { }
                
            };

            senderBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter) { 
                    sendData();
                    senderBox.Clear();
                }
            };
        }

        void FetchAvailablePorts()
        {
            ports = SerialPort.GetPortNames(); //We get the available COM ports
            Portsbox.Items.Clear();
            foreach (var port in ports)
            {
                Portsbox.Items.Add(port);
            }
        }

        void connect()
        {
            SerPort = new SerialPort(); //instantiate our serial port SerPort

            //hardcoding some parameters, check MSDN for more
            SerPort.BaudRate = 9600;
            SerPort.PortName = Portsbox.Text;
            SerPort.Parity = Parity.None;
            SerPort.DataBits = 8;
            SerPort.StopBits = StopBits.One;
            SerPort.ReadBufferSize = 200000000;
            SerPort.DataReceived += SerPort_DataReceived;

            try
            {
                SerPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error...!");
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) //Zorgt er voor dat enkel nummers in de textbox gezet kunnen worden
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumberValidationTextBoxNegatif(object sender, TextCompositionEventArgs e) //Zorgt er voor dat enkel nummers in de textbox gezet kunnen worden maar ook negatieve
        {
            box = (TextBox)sender;
            e.Handled = !regexN.IsMatch(box.Text.Insert(box.CaretIndex, e.Text));
        }

            private void SerPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                ReceivedData = SerPort.ReadLine(); //read the line from the serial port
            } catch { };
            Dispatcher.Invoke(new Action(ProcessingData)); //at each received line from serial port we "trigger" a new processingdata delegate
        }

        private void run()
        {
            try
            {
                xinterval = (this.ActualWidth - xAxisStart - 70) / Double.Parse(metingenBox.Text);
                if (xinterval < 0) xinterval = 0;
            }
            catch { }

            foreach (var channel in channels)
            {
                channel.setOrigin(this.ActualHeight, this.ActualWidth);
            }
            chartCanvas.Children.Clear();
            xAxisLine = new Line()
            {
                X1 = xAxisStart,
                Y1 = this.ActualHeight - yAxisStart,
                X2 = this.ActualWidth - 70,
                Y2 = this.ActualHeight - yAxisStart,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1,
            };
            yAxisLine = new Line()
            {
                X1 = xAxisStart,
                Y1 = yAxisStop,
                X2 = xAxisStart,
                Y2 = this.ActualHeight - yAxisStart,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1,
            };

            if (xAxisLine.X2 < xAxisLine.X1) xAxisLine.X2 = xAxisLine.X1;
            if (yAxisLine.Y2 < yAxisLine.Y1) yAxisLine.Y2 = yAxisLine.Y1;
            chartCanvas.Children.Add(xAxisLine);
            chartCanvas.Children.Add(yAxisLine);

            origin = new Point(xAxisLine.X1, yAxisLine.Y2);
            xrun();
            yrun();
        }
        private void xrun()
        {
            // x axis lines
            xValue = xinterval;
            xPoint = origin.X + xinterval;
            while (xPoint < xAxisLine.X2)
            {
                line = new Line()
                {
                    X1 = xPoint,
                    Y1 = yAxisLine.Y1,
                    X2 = xPoint,
                    Y2 = this.ActualHeight - yAxisStart,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1,
                    Opacity = 1,
                };

                if (line.Y2 < line.Y1) line.Y2 = line.Y1;
                chartCanvas.Children.Add(line);

                xPoint += xinterval;
                xValue += xinterval;
            }
        }

        // y axis lines
        private void yrun() {
           
            yPointInterval = (yAxisLine.Y2 - yAxisLine.Y1 - 1) / ysegments;
            if (yPointInterval < 1) yPointInterval = 1;
            yinterval = (ystop - ystart) / ysegments;
            if (yinterval == 0) { ysegments = 0; ySeg.Text = "0"; }
            yscale = (yAxisLine.Y2 - yAxisLine.Y1) / (ystop - ystart);


            yValue = ystart + yinterval;
            yPoint = origin.Y - yPointInterval;

            yTextBlock0 = new TextBlock() { Text = $"{ystart}" };
            chartCanvas.Children.Add(yTextBlock0);
            Canvas.SetLeft(yTextBlock0, origin.X - 30);
            Canvas.SetTop(yTextBlock0, origin.Y - 10);

            while (yPoint > yAxisLine.Y1)
                {
                    line = new Line()
                    {
                        X1 = xAxisStart,
                        Y1 = yPoint,
                        X2 = this.ActualWidth - 70,
                        Y2 = yPoint,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        Opacity = 1,
                    };

                if (line.X2 < line.X1) line.X2 = line.X1;
                chartCanvas.Children.Add(line);

                textBlock = new TextBlock() { Text = $"{yValue}" };
                chartCanvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, line.X1 - 30);
                Canvas.SetTop(textBlock, yPoint - 10);

                yPoint -= yPointInterval;
                yValue += yinterval;
            }
        }

        private void ProcessingData() //processing received data and plotting it on a chart
        {

            try
            {
                split = ReceivedData.Trim().Split(','); //split the data separated by tabs; split[3] -> split[col1], split[col2], split[col3]; split[i]
                count = split.Length;
                data = "\n";
                
                while(count != channels.Count)
                {
                    if (count < channels.Count) channels.RemoveAt(channels.Count - 1);
                    else channels.Add(new channel(channels.Count + 1, colors[colorCount++], this.ActualHeight, this.ActualWidth));
                    if(colorCount == colors.Length) colorCount = 0;
                }

                if (scaleOverride.IsChecked == false)
                {
                    yaxisScaling();
                }

                foreach (var ch in channels)
                {
                    ch.Line();
                    ch.drawingchannel(chartCanvas);
                }
                for(i = 0; i < count; i++)
                {
                    data += $"{split[i]}, ";
          
                }
                TextOut.Text += data;
                textOutLines++;
                while (textOutLines >= metingen) {
                    TextOut.Text = TextOut.Text.Remove(0, TextOut.Text.IndexOf("\n") + 1);
                    textOutLines--;
                }
                if(autoscroll.IsChecked== true) textScrol.ScrollToBottom();
            }
            catch { }   
        }

        private void yaxisScaling ()
        {

        }

        private void sendData() {
        
        }
    }


    public class Holder
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point Point { get; set; }

        public Holder()
        {
        }
    }

    public class Value
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Value(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
