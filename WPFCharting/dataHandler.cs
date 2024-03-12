using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFCharting
{
    public class channel
    {
        private int index;
        private System.Windows.Point point = new System.Windows.Point(MainWindow.xAxisStart, MainWindow.yAxisStart);
        private SolidColorBrush color = Brushes.Blue;
        private Polyline chartPolyline;
        private int offset;
        public int metingen = 50;

        private double[] temp = null;

        public double[] Serie = new double[50];

        public channel(int index, SolidColorBrush color)
        {
            this.index = index;
            this.color = color;
        }

        public void Sizing(int verzetting)
        {
            int Size = metingen;
            metingen = verzetting;
            if (Size < metingen)
            {
                temp = new double[metingen]; //maakt een rijdeljke array aan om data tijdelijk juist te zetten met de juiste limieten
                for (int i = 0; i < Size; i++)
                {
                    temp[i] = Serie[i];
                }
            }
            else if (Size > metingen)
            {
                offset = Size - metingen - 1;
                temp = new double[metingen];
                for (int i = 0; i < metingen - 1; i++)
                {
                    temp[i] = Serie[i + offset];
                };
            }
            Serie = temp; // geeft de hoofd array de juiste waardes en limieten
            temp = null;// leegt de tijdelijke array
        }

        public void Line()
        {
            for (int i = 0; i < metingen - 1; i++)
            {
                Serie[i] = Serie[i + 1];
            }
            Double.TryParse(MainWindow.split[index - 1], out Serie[metingen - 1]);
        }

        public void drawingchannel (Canvas chart) {
            chart.Children.Remove(chartPolyline);
            chartPolyline = new Polyline()
            {
                Stroke = color,
                StrokeThickness = 2,
            };
            chart.Children.Add(chartPolyline);
            for(int i = 0; i < metingen;i++)
            {
                point.X += i * MainWindow.interval;
                point.Y += Serie[i];
                chartPolyline.Points.Add(point);
            }
        }
    }
}
