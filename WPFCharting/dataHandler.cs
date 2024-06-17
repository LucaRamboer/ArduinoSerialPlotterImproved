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
        private readonly int index;
        private System.Windows.Point point = new System.Windows.Point(MainWindow.xAxisStart, MainWindow.yAxisStart);
        private readonly SolidColorBrush color = Brushes.Blue;
        private Polyline chartPolyline;
        private int offset;
        private double deltaValue;
        int i, Size;
        private int metingen = 50;
        private double height;
        private readonly Canvas chart;
        private TextBlock Limit;
        private bool offLowerLimit, offHigherLimit, limitNotified;

        private double[] temp = null;

        public double[] Serie = new double[50];

        public channel(int index, SolidColorBrush color, double height, Canvas chart, TextBlock Limit)
        {
            this.index = index;
            this.color = color;
            this.height = height - MainWindow.yAxisStart;
            this.chart = chart;
            this.Limit = Limit;
        }

        public void setOrigin (double height)
        {
            this.height = height - MainWindow.yAxisStart;
        }

        public void Sizing(int verzetting)
        {
            Size = metingen;
            metingen = verzetting;
            temp = new double[metingen];//maakt een rijdeljke array aan om data tijdelijk juist te zetten met de juiste limieten
            if (Size < metingen)//ziet of de array groter of kleiner moet worden om dan de juiste waardes op de juiste plek te zetten
            {
                for (i = 0; i < Size; i++)
                {
                    temp[i] = Serie[i];
                }
            }
            else if (Size > metingen)
            {
                offset = Size - metingen - 1;
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
            //schuift alle waardes door met 1 plaats
            for (i = 0; i < metingen - 1; i++)
            {
                Serie[i] = Serie[i + 1];
            }
            Double.TryParse(MainWindow.split[index - 1], out Serie[metingen - 1]);
        }

        public void drawingchannel () {
            chart.Children.Remove(chartPolyline);//haalt de vorige polyline weg van onze chart
            chartPolyline = new Polyline() //maakt een nieuwe polyline aan
            {
                Stroke = color,
                StrokeThickness = 2,
            };
            chart.Children.Add(chartPolyline);//voegt de nieuwe polyline toe aan onze chart

            //logia voor op elke x-waarde de juiste y waarde te plakken en deze dan vervolgens toe te voegen aan de polyline
            offLowerLimit = false;
            offHigherLimit = false; 
            for (i = 0; i < metingen; i++)
            {
                point.X = MainWindow.xAxisStart + i * MainWindow.xinterval;
                deltaValue = Serie[i] - MainWindow.ystart;
                if (deltaValue < 0) { point.Y = MainWindow.yAxisLine.Y2 + 5; offLowerLimit = true; }
                else point.Y = height - (deltaValue * MainWindow.yscale);
                chartPolyline.Points.Add(point);
            }
            if ((offLowerLimit || offHigherLimit))
            {
                if (!limitNotified) { 
                    Limit.Text += $"{index} ";
                    limitNotified = true;
                }
            }else if(limitNotified)
            {
                Limit.Text = Limit.Text.Replace($" {index} ", " ");
                limitNotified = false;
            }

        }

        public void remove()
        {
            chart.Children.Remove(chartPolyline);//verwijdert de polyline van onze chart
        }
    }
}
