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
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;

namespace WPFCharting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i;
        private bool scaleChanging, savetofile, connected;
        public static Line xAxisLine, yAxisLine, line;
        public static double xAxisStart = 150, yAxisStart = 250, yAxisStop = 50;
        public static double xinterval { get; set; } = 50;
        public static double yinterval { get; set; } = 50;
        public double yPointInterval;
        public int ysegments = 10;
        public static double ystart = 0, ystop = 50;
        public double yboxMin = 0, yboxMax = 50;
        double yPoint, xPoint, yValue, MaxIndex, MinIndex;
        public static double yscale;
        private double tempD;
        private int count;
        private int colorCount;
        private int textOutLines, metingen = 50;
        private string ReceivedData;
        public string data;
        private SerialPort SerPort;
        readonly Regex regexN = new Regex("^-{0,1}[0-9]{0,}$");
        readonly Regex regex = new Regex("[^0-9]+");
        readonly Regex regexIn = new Regex("[0-9.]+");
        TextBlock yTextBlock0, textBlock;
        TextBox box;
        private Point origin;
        string pathFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PlotOutput.txt");
        public static string[] split = new string[] { };
        string[] output = new string[1];
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
        System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog(); //defines folder dialog
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer(); //defines interval timer


        public MainWindow()
        {
            InitializeComponent();
            FetchAvailablePorts();
            this.StateChanged += (sender, e) =>
            {
                run();
            };
            
            this.SizeChanged += (sender, e) =>
            {
                run();
            };
            
            connectButton.Click += (sender, e) =>
            {
                if (connected) disconnect(); else connect();
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
                try
                {
                    yboxMax = Double.Parse(YMax.Text);
                    if (scaleOverride.IsChecked == true)
                    {
                        ystop = yboxMax;
                        run();
                    }
                }
                catch { }
            };

            YMin.TextChanged += (sender, e) =>
            {
                try
                {
                    yboxMin = Double.Parse(YMin.Text);
                    if (scaleOverride.IsChecked == true)
                    {
                        ystart = yboxMin;
                        run();
                    }
                }
                catch { }
            };

            scaleOverride.Checked += (sender, e) =>
            {
                ystop = yboxMax;
                ystart = yboxMin;
                run();
            };

            scaleOverride.Unchecked += (sender, e) => {
                findLimits();
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

            SaveToFile.Checked += (sender, e) => {
                savetofile = true;
            };

            SaveToFile.Unchecked += (sender, e) => {
                savetofile = false;
            };

            pathSelector.Click += (sender, e) =>
            {
                selectPath();
            };

            //puts the right settings on the iterval timer
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        void FetchAvailablePorts()
        {
            Portsbox.Items.Clear();
            ports = SerialPort.GetPortNames(); //We get the available COM ports
            foreach (var port in ports)
            {
                Portsbox.Items.Add(port);
            }
        }

        void connect()
        {
            //hardcoding some parameters, check MSDN for more

            try
            {
                SerPort = new SerialPort(); //instantiate our serial port SerPort
                SerPort.BaudRate = 9600;
                SerPort.PortName = Portsbox.Text;
                SerPort.Parity = Parity.None;
                SerPort.DataBits = 8;
                SerPort.StopBits = StopBits.One;
                SerPort.ReadBufferSize = 200000000;
                SerPort.DataReceived += SerPort_DataReceived;
                SerPort.Open();
                connected = true;
                connectButton.Content = "disconnect";
                refreshButton.IsEnabled = false;
                Portsbox.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error...!");
                if(ex.Message.EndsWith("does not exist.")) FetchAvailablePorts();
            }
            
        }

        void disconnect() {
            try
            {
                SerPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error...!");
            }
            connected = false;
            connectButton.Content = "connect";
            refreshButton.IsEnabled = true;
            Portsbox.IsEnabled = true;
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
            xinterval = (this.ActualWidth - xAxisStart - 70) / (metingen - 1);
            if (xinterval < 0) xinterval = 0;
            
            foreach (var channel in channels)
            {
                channel.setOrigin(this.ActualHeight);
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
            foreach (var ch in channels) ch.drawingchannel();
        }
        private void xrun()
        {
            // x axis lines
            xPoint = origin.X + xinterval;
             for(i = 0; i < metingen - 1; i++)
            {
                line = new Line()
                {
                    X1 = xPoint,
                    Y1 = yAxisLine.Y1,
                    X2 = xPoint,
                    Y2 = yAxisLine.Y2,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1,
                    Opacity = 1,
                };

                if (line.Y2 < line.Y1) { line = null; break; }
                chartCanvas.Children.Add(line);

                xPoint += xinterval;
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

                if (line.X2 < line.X1) { line = null; break; }
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
                if (regexIn.IsMatch(split[0]))
                {
                    for(i = 0; i < count; i++) { split[i] = split[i].Replace('.', ','); }
                    while (count != channels.Count)
                    {
                        if (count < channels.Count)
                        {
                            channels[channels.Count - 1].remove();
                            channels.RemoveAt(channels.Count - 1);
                            if (scaleOverride.IsChecked == true) findLimits();
                        }
                        else channels.Add(new channel(channels.Count + 1, colors[colorCount++], this.ActualHeight, chartCanvas));
                        if (colorCount == colors.Length) colorCount = 0;
                    }

                    foreach (var ch in channels)
                    {
                        ch.Line();
                        ch.drawingchannel();
                    }


                    for (i = 0; i < count; i++)
                    {
                        data += $"{split[i]}, ";
                        if (scaleOverride.IsChecked == false)
                        {
                            tempD = Double.Parse(split[i]);
                            if (tempD >= ystop)
                            {
                                MaxIndex = metingen;
                                scaleChanging = true;
                                if (tempD != ystop)
                                {
                                    ystop = tempD;
                                }
                            }
                            else if (tempD <= ystart)
                            {
                                MinIndex = metingen;
                                scaleChanging = true;
                                if (tempD != ystart)
                                {
                                    ystart = tempD;
                                }
                            }
                        }
                    }

                    if (scaleOverride.IsChecked == false)
                    {
                        MaxIndex--;
                        MinIndex--;
                        if (MaxIndex < 0)
                        {
                            findLimits(findmin: false);
                        }
                        if (MinIndex < 0)
                        {
                            findLimits(findmax: false);
                        }
                    }


                    TextOut.Text += data;
                    output[0] = data;
                    if (savetofile)
                    {
                        if (File.Exists(pathFile)) {
                            File.AppendAllLines(pathFile, output); 
                        } else { 
                            File.WriteAllText(pathFile, data);
                        }
                    }
                    textOutLines++;
                    while (textOutLines >= metingen)
                    {
                        TextOut.Text = TextOut.Text.Remove(0, TextOut.Text.IndexOf("\n") + 1);
                        textOutLines--;
                    }

                    if ((scaleOverride.IsChecked == false) && scaleChanging)
                    {
                        scaleChanging = false;
                        run();
                    }

                } else
                {
                    TextOut.Text += "\n" + ReceivedData.Remove(ReceivedData.Length - 1);
                }
                if (autoscroll.IsChecked == true) textScrol.ScrollToBottom();
            }
            catch { }   
        }

        private void findLimits(bool findmax = true, bool findmin = true) { 
            if(findmax)
            {
                ystop = channels[0].Serie[0];
                foreach (var ch in channels)
                {
                    for (i = 0; i < metingen; i++)
                    {
                        if (ch.Serie[i] > ystop) { ystop = ch.Serie[i]; MaxIndex = i; }
                    }
                }
            }
            if(findmin)
            {
                ystart = channels[0].Serie[0];
                foreach (var ch in channels)
                {
                    for (i = 0; i < metingen; i++)
                    {
                        if (ch.Serie[i] < ystart) { ystart = ch.Serie[i]; MinIndex = i; }
                    }
                }
            }
            scaleChanging = true;
        }

        private void sendData() {
            if (connected)
            {
                try
                {
                    SerPort.WriteLine(senderBox.Text);
                }
                catch (Exception ex)
                {
                    if (!SerPort.IsOpen) losconection(); 
                    else MessageBox.Show(ex.Message, "Error...!");
                }
            }
            else {
                MessageBox.Show("Not connected to a port", "Connection");
            }
        }

        void selectPath()
        {
            openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                pathFile = System.IO.Path.Combine(openFileDlg.SelectedPath, "PlotOutput.txt");
            }
            
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(connected && !SerPort.IsOpen)//checks every tick if the connection isn't lost
            {
                losconection();
            }
        }

        //notifys the user and sets the right vars to the right values for the los of connection
        void losconection()
        {
            connected = false;
            connectButton.Content = "connect";
            refreshButton.IsEnabled = true;
            Portsbox.IsEnabled = true;
            FetchAvailablePorts();
            MessageBox.Show("Connection to port lost!", "Connection");
        }
    }
}
