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

namespace WPFCharting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Line xAxisLine, yAxisLine;
        public static double xAxisStart = 140, yAxisStart = 100;
        public static double interval { get; } = 50;
        private int count;
        private int colorCount;
        private string ReceivedData;
        private Polyline chartPolyline;
        private SerialPort SerPort;

        private Point origin;
        public static string[] split = new string[] { };
        private List<Holder> holders;
        private List<Value> values;
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
            holders = new List<Holder>();
            this.StateChanged += (sender, e) => {
                run();
            };

            this.SizeChanged += (sender, e) => {
                run();
            };
            FetchAvailablePorts();
            connectButton.Click += (sender, e) => {
                connect();
            };
        }

        void FetchAvailablePorts()
        {
            String[] ports = SerialPort.GetPortNames(); //We get the available COM ports
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

        private void SerPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReceivedData = SerPort.ReadLine(); //read the line from the serial port

            Dispatcher.Invoke(new Action(ProcessingData)); //at each received line from serial port we "trigger" a new processingdata delegate
        }

        private void run()
        {
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
                Y1 = yAxisStart - 50,
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

            var xTextBlock0 = new TextBlock() { Text = $"{0}" };
            chartCanvas.Children.Add(xTextBlock0);
            Canvas.SetLeft(xTextBlock0, origin.X);
            Canvas.SetTop(xTextBlock0, origin.Y + 5);

            // y axis lines
            var xValue = interval;
            double xPoint = origin.X + interval;
            while (xPoint < xAxisLine.X2)
            {
                var line = new Line()
                {
                    X1 = xPoint,
                    Y1 = yAxisStart - 50,
                    X2 = xPoint,
                    Y2 = this.ActualHeight - yAxisStart,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1,
                    Opacity = 1,
                };

                if (line.Y2 < line.Y1) line.Y2 = line.Y1;
                chartCanvas.Children.Add(line);

                var textBlock = new TextBlock { Text = $"{xValue}", };

                chartCanvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, xPoint - 12.5);
                Canvas.SetTop(textBlock, line.Y2 + 5);

                xPoint += interval;
                xValue += interval;
            }


            var yTextBlock0 = new TextBlock() { Text = $"{0}" };
            chartCanvas.Children.Add(yTextBlock0);
            Canvas.SetLeft(yTextBlock0, origin.X - 20);
            Canvas.SetTop(yTextBlock0, origin.Y - 10);

            // x axis lines
            var yValue = yAxisStart;
            double yPoint = origin.Y - interval;
            while (yPoint > yAxisLine.Y1)
            {
                var line = new Line()
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

                var textBlock = new TextBlock() { Text = $"{yValue}" };
                chartCanvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, line.X1 - 30);
                Canvas.SetTop(textBlock, yPoint - 10);

                yPoint -= interval;
                yValue += interval;
            }
            /*double x = 0, y = 0;
            xPoint = origin.X;
            yPoint = origin.Y;
            while (xPoint < xAxisLine.X2)
            {
                while (yPoint > yAxisLine.Y1)
                {
                    var holder = new Holder()
                    {
                        X = x,
                        Y = y,
                        Point = new Point(xPoint, yPoint),
                    };

                    holders.Add(holder);

                    yPoint -= interval;
                    y += interval;
                }

                xPoint += interval;
                yPoint = origin.Y;
                x += 100;
                y = 0;
            }

            // polyline
            chartPolyline = new Polyline()
            {
                Stroke = new SolidColorBrush(Color.FromRgb(68, 114, 196)),
                StrokeThickness = 5,
            };
            chartCanvas.Children.Add(chartPolyline);

            // showing where are the connections points
            foreach (var holder in holders)
            {
                Ellipse oEllipse = new Ellipse()
                {
                    Fill = Brushes.Red,
                    Width = 10,
                    Height = 10,
                    Opacity = 0,
                };

                chartCanvas.Children.Add(oEllipse);
                Canvas.SetLeft(oEllipse, holder.Point.X - 5);
                Canvas.SetTop(oEllipse, holder.Point.Y - 5);
            }

            // add connection points to polyline
            foreach (var value in values)
            {
                var holder = holders.FirstOrDefault(h => h.X == value.X && h.Y == value.Y);
                if (holder != null)
                    chartPolyline.Points.Add(holder.Point);
            }*/
        }

        private void ProcessingData() //processing received data and plotting it on a chart
        {

            try
            {
                split = ReceivedData.Split('/'); //split the data separated by tabs; split[3] -> split[col1], split[col2], split[col3]; split[i]
                count = split.Length;
                
                while(count != channels.Count)
                {
                    if (count < channels.Count) channels.RemoveAt(channels.Count - 1);
                    else channels.Add(new channel(channels.Count + 1, colors[colorCount++]));
                    if(colorCount == colors.Length) colorCount = 0;
                }
                foreach(var ch in channels)
                {
                    ch.Line();
                    ch.drawingchannel(chartCanvas);
                }
                /*
                //handle the output
                foutput = _split1.ToString() + "\t" + _split2.ToString() + "\t" + _split3.ToString() + Environment.NewLine;
                //we put together the variables into a string (foutput) again

                if (FileSavingCheckBox.Checked == true) //file saving, same folder as executable
                {
                    using (StreamWriter sw = File.AppendText("Outputfile.txt"))//appendtext = the previous file will be continued
                    {
                        sw.Write(foutput); //write the content of foutput into the file
                    }
                }
                */
            }
            catch { }   
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
