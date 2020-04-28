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
        private List<List<Vertex>> paths = new List<List<Vertex>>();
        private HashSet<(double, double, double, double)> drawnLines = new HashSet<(double, double, double, double)>();
        private Vertex[,] vertMatrix;
        private double xScale;
        private double yScale;
        private double size = 10;
        private double xMin;
        private double yMin;

        public MainWindow()
        {
            InitializeComponent();
            MatrixInit();
            LoadXml();
            SetScale();
            SetCoords();
        }

        private void MatrixInit()
        {
            vertMatrix = new Vertex[(int)(canv.Width / (size / 2)) + 1, (int)(canv.Height / (size / 2)) + 1];
            for (int i = 0; i < vertMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < vertMatrix.GetLength(1); j++)
                {
                    vertMatrix[i, j] = new Vertex(i, j, char.MinValue);
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



        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                SetMatrixElement(item.X, item.Y);
            }
            foreach (var item in nodeEntities)
            {
                double x = Util.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Util.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Util.FindClosestXY(x, y, size);
                SetMatrixElement(item.X, item.Y);
            }
            foreach (var item in switchEntities)
            {
                double x = Util.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Util.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Util.FindClosestXY(x, y, size);
                SetMatrixElement(item.X, item.Y);
            }
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
                var root = vertMatrix[(int)(x1 / (size / 2)), (int)(y1 / (size / 2))];
                var end = vertMatrix[(int)(x2 / (size / 2)), (int)(y2 / (size / 2))];

                var path = Util.SearchBFS(vertMatrix, root, end, true);
                if (path == null)
                {
                    path = Util.SearchBFS(vertMatrix, root, end, false);
                }
                paths.Add(path);
            }
        }

        private void SetMatrixElement(double x, double y)
        {
            int xx = (int)(x / (size / 2));
            int yy = (int)(y / (size / 2));
            vertMatrix[xx, yy].Data = 'o';
            vertMatrix[xx, yy].X = xx;
            vertMatrix[xx, yy].Y = yy;
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
            double x1;
            double y1;
            double x2;
            double y2;
            foreach (var path in paths)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    x1 = (path[i].X * (size / 2)) + (5 / 2);
                    y1 = (path[i].Y * (size / 2)) + (5 / 2);
                    x2 = (path[i + 1].X * (size / 2)) + (5 / 2);
                    y2 = (path[i + 1].Y * (size / 2)) + (5 / 2);

                    if (!(drawnLines.Contains((x1, y1, x2, y2)) || drawnLines.Contains((x2, y2, x1, y1))))
                    {
                        var l = new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 0.5,
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2
                        };
                        drawnLines.Add((x1, y1, x2, y2));
                        canv.Children.Add(l);

                        if (drawnLines.Select((item) => (item.Item1, item.Item2)).Where((item) => item == (x1, y1)).Count() >= 2)
                        {
                            DrawCross(x1, y1);
                        }
                        if (drawnLines.Select((item) => (item.Item1, item.Item2)).Where((item) => item == (x2, y2)).Count() >= 2)
                        {
                            DrawCross(x2, y2);
                        }
                        if (drawnLines.Select((item) => (item.Item3, item.Item4)).Where((item) => item == (x1, y1)).Count() >= 2)
                        {
                            DrawCross(x1, y1);
                        }
                        if (drawnLines.Select((item) => (item.Item3, item.Item4)).Where((item) => item == (x2, y2)).Count() >= 2)
                        {
                            DrawCross(x2, y2);
                        }
                    }
                }
            }
        }

        private void DrawCross(double x1, double y1)
        {
            var element = new Ellipse() { Width = 2.5, Height = 2.5, Fill = Brushes.Purple };
            Canvas.SetLeft(element, x1 - (2.5 / 2));
            Canvas.SetTop(element, y1 - (2.5 / 2));
            canv.Children.Add(element);
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
