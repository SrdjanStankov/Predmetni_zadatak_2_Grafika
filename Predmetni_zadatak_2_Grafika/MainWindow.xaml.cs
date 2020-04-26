using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Predmetni_zadatak_2_Grafika.Model;
using Predmetni_zadatak_2_Grafika.Services;

namespace Predmetni_zadatak_2_Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// substation, node, switch == cvorovi
    /// line entity se gleda samo pocetna i krajnja tacka
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<SubstationEntity> substationEntities = new List<SubstationEntity>();
        private List<NodeEntity> nodeEntities = new List<NodeEntity>();
        private List<SwitchEntity> switchEntities = new List<SwitchEntity>();
        private List<LineEntity> lineEntities = new List<LineEntity>();
        private char[,] matrix;
        private Vertex[,] vertMatrix;
        private double xScale;
        private double yScale;
        private double size = 10;
        private double xMin;
        private double yMin;

        public MainWindow()
        {
            InitializeComponent();

            matrix = new char[(int)(canv.Width / (size / 2)) + 1, (int)(canv.Height / (size / 2)) + 1];

            LoadXml();
            SetScale();
            SetCoords();

            vertMatrix = new Vertex[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    vertMatrix[i, j] = new Vertex(i, j, matrix[i, j]);
                }
            }
        }

        private void SetScale()
        {
            xMin = Math.Min(Math.Min(substationEntities.Min((item) => item.X), nodeEntities.Min((item) => item.X)), switchEntities.Min((item) => item.X));
            yMin = Math.Min(Math.Min(substationEntities.Min((item) => item.Y), nodeEntities.Min((item) => item.Y)), switchEntities.Min((item) => item.Y));
            xScale = canv.Width / (Math.Max(Math.Max(substationEntities.Max((item) => item.X), nodeEntities.Max((item) => item.X)), switchEntities.Max((item) => item.X)) - xMin);
            yScale = canv.Height / (Math.Max(Math.Max(substationEntities.Max((item) => item.Y), nodeEntities.Max((item) => item.Y)), switchEntities.Max((item) => item.Y)) - yMin);
        }

        private void LoadXml()
        {
            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            Util.AddEntities(substationEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity"));
            Util.AddEntities(nodeEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity"));
            Util.AddEntities(switchEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity"));
            Util.AddEntities(lineEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity"));
        }



        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawElements();
        }

        private void SetCoords()
        {
            foreach (var item in substationEntities)
            {
                double x = Util.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Util.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Util.FindClosestXY(x, y, size);
                matrix[(int)(item.X / (size / 2)), (int)(item.Y / (size / 2))] = 'o';
            }
            foreach (var item in nodeEntities)
            {
                double x = Util.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Util.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Util.FindClosestXY(x, y, size);
                matrix[(int)(item.X / (size / 2)), (int)(item.Y / (size / 2))] = 'o';
            }
            foreach (var item in switchEntities)
            {
                double x = Util.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Util.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Util.FindClosestXY(x, y, size);
                matrix[(int)(item.X / (size / 2)), (int)(item.Y / (size / 2))] = 'o';
            }
        }

        private void DrawElements()
        {
            foreach (var item in substationEntities)
            {
                var element = new Ellipse() { Width = 5, Height = 5, Fill = Brushes.Red };
                Canvas.SetLeft(element, item.X);
                Canvas.SetTop(element, item.Y);
                canv.Children.Add(element);
            }

            foreach (var item in nodeEntities)
            {
                var element = new Ellipse() { Width = 5, Height = 5, Fill = Brushes.Blue };
                Canvas.SetLeft(element, item.X);
                Canvas.SetTop(element, item.Y);
                canv.Children.Add(element);
            }

            foreach (var item in switchEntities)
            {
                var element = new Ellipse() { Width = 5, Height = 5, Fill = Brushes.Green };
                Canvas.SetLeft(element, item.X);
                Canvas.SetTop(element, item.Y);
                canv.Children.Add(element);
            }
            int counter = 0;
            foreach (var item in lineEntities)
            {
                (double x1, double y1) = FindElemt(item.FirstEnd);
                (double x2, double y2) = FindElemt(item.SecondEnd);
                if (x1 == 0 || x2 == 0 || y1 == 0 || y2 == 0)
                {
                    continue;
                }

                foreach (var vert in vertMatrix)
                {
                    vert.Parent = null;
                }
                // Total 2223
                var path = Util.SearchBFS(vertMatrix, vertMatrix[(int)(x1 / (size / 2)), (int)(y1 / (size / 2))], vertMatrix[(int)(x2 / (size / 2)), (int)(y2 / (size / 2))]);

                if (path != null)
                {
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        var l = new Line
                        {
                            Stroke = Brushes.Black,
                            X1 = (path[i].X * (size / 2)) + (5 / 2),
                            Y1 = (path[i].Y * (size / 2)) + (5 / 2),

                            X2 = (path[i + 1].X * (size / 2)) + (5 / 2),
                            Y2 = (path[i + 1].Y * (size / 2)) + (5 / 2),
                            StrokeThickness = 1
                        };

                        canv.Children.Add(l);
                    }
                }
                else
                {
                    counter++;
                }
            }
            Console.WriteLine($"Nulls: {counter}");
        }

        private (double, double) FindElemt(long id)
        {
            return substationEntities.Find((item) => item.Id == id) != null
                ? (substationEntities.Find((item) => item.Id == id).X + (5 / 2), substationEntities.Find((item) => item.Id == id).Y + (5 / 2))
                : nodeEntities.Find((item) => item.Id == id) != null
                ? (nodeEntities.Find((item) => item.Id == id).X + (5 / 2), nodeEntities.Find((item) => item.Id == id).Y + (5 / 2))
                : switchEntities.Find((item) => item.Id == id) != null
                ? (switchEntities.Find((item) => item.Id == id).X + (5 / 2), switchEntities.Find((item) => item.Id == id).Y + (5 / 2)) : (0, 0);
        }
    }
}
