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
        private double xScale;
        private double yScale;
        private double size = 10;
        private double xMin;
        private double yMin;

        public MainWindow()
        {
            InitializeComponent();

            LoadXml();
            SetScale();
            SetCoords();
        }

        private void SetScale()
        {
            xMin = Math.Min(Math.Min(substationEntities.Min((item) => item.X), nodeEntities.Min((item) => item.X)), switchEntities.Min((item) => item.X));
            yMin = Math.Min(Math.Min(substationEntities.Min((item) => item.Y), nodeEntities.Min((item) => item.Y)), switchEntities.Min((item) => item.Y));
            xScale = canv.Width / (Math.Max(Math.Min(substationEntities.Max((item) => item.X), nodeEntities.Max((item) => item.X)), switchEntities.Max((item) => item.X)) - xMin);
            yScale = canv.Height / (Math.Max(Math.Min(substationEntities.Max((item) => item.Y), nodeEntities.Max((item) => item.Y)), switchEntities.Max((item) => item.Y)) - yMin);
        }

        private void LoadXml()
        {
            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            Common.AddEntities(substationEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity"));
            Common.AddEntities(nodeEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity"));
            Common.AddEntities(switchEntities, doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity"));
        }



        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawElements();
        }

        private void SetCoords()
        {
            foreach (var item in substationEntities)
            {
                double x = Common.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Common.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Common.FindClosestXY(x, y, size);
            }
            foreach (var item in nodeEntities)
            {
                double x = Common.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Common.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Common.FindClosestXY(x, y, size);
            }
            foreach (var item in switchEntities)
            {
                double x = Common.ConvertToCanvas(item.X, xScale, xMin, size, canv.Width);
                double y = Common.ConvertToCanvas(item.Y, yScale, yMin, size, canv.Width);
                (item.X, item.Y) = Common.FindClosestXY(x, y, size);
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
        }
    }
}
