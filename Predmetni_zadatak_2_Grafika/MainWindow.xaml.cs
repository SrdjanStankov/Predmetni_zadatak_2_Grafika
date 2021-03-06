﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using Predmetni_zadatak_2_Grafika.Model;
using Predmetni_zadatak_2_Grafika.Services;

namespace Predmetni_zadatak_2_Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<SubstationEntity> substationEntities = new List<SubstationEntity>();
        private List<NodeEntity> nodeEntities = new List<NodeEntity>();
        private List<SwitchEntity> switchEntities = new List<SwitchEntity>();
        private List<LineEntity> lineEntities = new List<LineEntity>();
        private List<List<Vertex>> paths = new List<List<Vertex>>();
        private HashSet<(double, double, double, double)> drawnLines = new HashSet<(double, double, double, double)>();
        private Dictionary<(double x, double y), int> pointsOnSameCoords = new Dictionary<(double x, double y), int>();
        private List<(Shape shape, Brush original)> highlighted = new List<(Shape shape, Brush original)>();
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
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetCoords();
            DrawElements();
            await Dispatcher.InvokeAsync(SetZIndex);
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

        private void LoadXml()
        {
            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            Util.AddEntities(substationEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity"));
            Util.AddEntities(nodeEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity"));
            Util.AddEntities(switchEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity"));
            Util.AddLineEntities(lineEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity"));
        }

        private void SetScale()
        {
            xMin = Math.Min(Math.Min(substationEntities.Min((item) => item.X), nodeEntities.Min((item) => item.X)), switchEntities.Min((item) => item.X));
            yMin = Math.Min(Math.Min(substationEntities.Min((item) => item.Y), nodeEntities.Min((item) => item.Y)), switchEntities.Min((item) => item.Y));
            xScale = canv.Width / (Math.Max(Math.Max(substationEntities.Max((item) => item.X), nodeEntities.Max((item) => item.X)), switchEntities.Max((item) => item.X)) - xMin);
            yScale = canv.Height / (Math.Max(Math.Max(substationEntities.Max((item) => item.Y), nodeEntities.Max((item) => item.Y)), switchEntities.Max((item) => item.Y)) - yMin);
        }

        private void SetCoords()
        {
            substationEntities.ForEach((item) => CalculateCoords(item));
            nodeEntities.ForEach((item) => CalculateCoords(item));
            switchEntities.ForEach((item) => CalculateCoords(item));

            foreach (var item in lineEntities)
            {
                (double x1, double y1) = FindElemt(item.FirstEnd);
                (double x2, double y2) = FindElemt(item.SecondEnd);
                if (x1 == 0 || x2 == 0 || y1 == 0 || y2 == 0)
                {
                    continue;
                }

                var root = vertMatrix[(int)(x1 / (size / 2)), (int)(y1 / (size / 2))];
                var end = vertMatrix[(int)(x2 / (size / 2)), (int)(y2 / (size / 2))];

                var path = Util.SearchBFS(vertMatrix, root, end, true, item) ?? Util.SearchBFS(vertMatrix, root, end, false, item);

                paths.Add(path);
                path.ForEach((item) => item.Parent = null);
            }
        }

        private void CalculateCoords(PowerEntity item)
        {
            double x = Util.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
            double y = Util.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
            (item.X, item.Y) = Util.FindClosestXY(x, y, size);
            SetMatrixElement(item.X, item.Y);
        }

        private void SetMatrixElement(double x, double y)
        {
            int xx = (int)(x / (size / 2));
            int yy = (int)(y / (size / 2));
            vertMatrix[xx, yy].Data = 'o';
            vertMatrix[xx, yy].X = xx;
            vertMatrix[xx, yy].Y = yy;
        }

        private (double x, double y) FindElemt(long id)
        {
            var sub = substationEntities.Find((item) => item.Id == id);
            if (sub != null)
            {
                return (sub.X + (5 / 2), sub.Y + (5 / 2));
            }
            var nod = nodeEntities.Find((item) => item.Id == id);
            if (nod != null)
            {
                return (nod.X + (5 / 2), nod.Y + (5 / 2));
            }
            var swit = switchEntities.Find((item) => item.Id == id);
            return swit != null ? (swit.X + (5 / 2), swit.Y + (5 / 2)) : ((double x, double y))(0, 0);
        }

        private void DrawElements()
        {
            substationEntities.ForEach((item) => DrawSingleElement(item, Brushes.Red));
            nodeEntities.ForEach((item) => DrawSingleElement(item, Brushes.Blue));
            switchEntities.ForEach((item) => DrawSingleElement(item, Brushes.Green));

            double x1, y1, x2, y2;
            foreach (var path in paths)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    (x1, y1, x2, y2) = CalculateCoordsForLine(path, i);

                    path[i].ConnectedTo.Add(vertMatrix[path[0].X, path[0].Y].Self);
                    path[i].ConnectedTo.Add(vertMatrix[path[path.Count - 1].X, path[path.Count - 1].Y].Self);

                    if (!(drawnLines.Contains((x1, y1, x2, y2)) || drawnLines.Contains((x2, y2, x1, y1))))
                    {
                        var l = new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            ToolTip = $"ID: {path[i].Line.Id}{Environment.NewLine}Name: {path[i].Line.Name}"
                        };
                        l.MouseRightButtonDown += OnRightClickHighlightNodes;
                        drawnLines.Add((x1, y1, x2, y2));
                        canv.Children.Add(l);

                        if (!pointsOnSameCoords.ContainsKey((x1, y1)))
                        {
                            pointsOnSameCoords.Add((x1, y1), 1);
                        }
                        else
                        {
                            pointsOnSameCoords[(x1, y1)]++;
                        }

                        if (!pointsOnSameCoords.ContainsKey((x2, y2)))
                        {
                            pointsOnSameCoords.Add((x2, y2), 1);
                        }
                        else
                        {
                            pointsOnSameCoords[(x2, y2)]++;
                        }

                        if (pointsOnSameCoords[(x1, y1)] > 2)
                        {
                            DrawCross(x1, y1);
                        }
                        if (pointsOnSameCoords[(x2, y2)] > 2)
                        {
                            DrawCross(x2, y2);
                        }
                    }
                }
            }
        }

        private void DrawSingleElement(PowerEntity item, Brush color)
        {
            var element = new Ellipse() { Width = 5, Height = 5, Fill = color, ToolTip = $"ID: {item.Id}{Environment.NewLine}Name: {item.Name}" };
            if (item is SwitchEntity)
            {
                element.ToolTip += $"{Environment.NewLine}Status: {(item as SwitchEntity).Status}";
            }
            Canvas.SetLeft(element, item.X);
            Canvas.SetTop(element, item.Y);
            vertMatrix[(int)item.X / (int)(size / 2), (int)item.Y / (int)(size / 2)].Self = element;
            element.MouseLeftButtonDown += OnClickScale;
            canv.Children.Add(element);
        }

        private void OnClickScale(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var elipse = (Shape)sender;
            var ani = new DoubleAnimation(elipse.ActualWidth * 3, TimeSpan.FromSeconds(.25))
            {
                AutoReverse = true
            };
            var transform = new ScaleTransform(1, 1, 2.5, 2.5);
            elipse.RenderTransform = transform;
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, ani);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, ani);
        }

        private (double x1, double y1, double x2, double y2) CalculateCoordsForLine(List<Vertex> path, int i)
        {
            double x1 = (path[i].X * (size / 2)) + (5 / 2);
            double y1 = (path[i].Y * (size / 2)) + (5 / 2);
            double x2 = (path[i + 1].X * (size / 2)) + (5 / 2);
            double y2 = (path[i + 1].Y * (size / 2)) + (5 / 2);
            return (x1, y1, x2, y2);
        }

        private void OnRightClickHighlightNodes(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var line = (Line)sender;
            int x = (int)(line.X1 - (5 / 2)) / (int)(size / 2);
            int y = (int)(line.Y1 - (5 / 2)) / (int)(size / 2);

            foreach (var item in highlighted)
            {
                item.shape.Fill = item.original;
            }
            highlighted.Clear();

            foreach (var item in vertMatrix[x, y].ConnectedTo)
            {
                highlighted.Add((item, item.Fill));
                item.Fill = Brushes.Magenta;
            }
        }

        private void DrawCross(double x1, double y1)
        {
            var element = new Ellipse() { Width = 2.5, Height = 2.5, Fill = Brushes.Purple };
            Canvas.SetLeft(element, x1 - (2.5 / 2));
            Canvas.SetTop(element, y1 - (2.5 / 2));
            canv.Children.Add(element);
        }

        private void SetZIndex()
        {
            foreach (object item in canv.Children)
            {
                if (item is Ellipse)
                {
                    if ((item as Ellipse).Width == 5)
                    {
                        Panel.SetZIndex(item as UIElement, 5);
                    }
                }
            }
        }
    }
}
