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
        int i, Size;
        public int metingen = 50;
        private double height, width;

        private int[] temp = null;

        public int[] Serie = new int[50];

        public channel(int index, SolidColorBrush color, double height, double width)
        {
            this.index = index;
            this.color = color;
            this.height = height - MainWindow.yAxisStart;
            this.width = width - 70;
        }

        public void setOrigin (double height, double width)
        {
            this.height = height - MainWindow.yAxisStart;
            this.width = width - 70;
        }

        public void Sizing(int verzetting)
        {
            Size = metingen;
            metingen = verzetting;
            if (Size < metingen)
            {
                temp = new int[metingen]; //maakt een rijdeljke array aan om data tijdelijk juist te zetten met de juiste limieten
                for (i = 0; i < Size; i++)
                {
                    temp[i] = Serie[i];
                }
            }
            else if (Size > metingen)
            {
                offset = Size - metingen - 1;
                temp = new int[metingen];
                for (i = 0; i < metingen - 1; i++)
                {
                    temp[i] = Serie[i + offset];
                };
            }
            Serie = temp; // geeft de hoofd array de juiste waardes en limieten
            temp = null;// leegt de tijdelijke array
        }

        public void Line()
        {
            for (i = 0; i < metingen - 1; i++)
            {
                Serie[i] = Serie[i + 1];
            }
            Int32.TryParse(MainWindow.split[index - 1], out Serie[metingen - 1]);
        }

        public void drawingchannel (Canvas chart) {
            chart.Children.Remove(chartPolyline);
            chartPolyline = new Polyline()
            {
                Stroke = color,
                StrokeThickness = 2,
            };
            chart.Children.Add(chartPolyline);
            for (i = 0; i < metingen;i++)
            {
                point.X = MainWindow.xAxisStart + i * MainWindow.xinterval;
                point.Y = height - Serie[i];
                if (point.X > width) break;
                chartPolyline.Points.Add(point);
            }
        }
    }
}
