using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Predmetni_zadatak_2_Grafika.Model;

namespace Predmetni_zadatak_2_Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// substation, node, switch == cvorovi
    /// line entity se gleda samo pocetna i krajnja tacka
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ROW_HEIGHT_COUNT = 100;
        private HashSet<(uint, uint)> usedCoords = new HashSet<(uint, uint)>();

        public MainWindow()
        {
            InitializeComponent();
            window.WindowState = WindowState.Maximized;
        }

        private void LoadXml()
        {
            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            var nodeList = doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            var substations = new List<SubstationEntity>();
            CreateElementsOnGui<SubstationEntity, Rectangle>(nodeList, substations, Brushes.Red);

            nodeList = doc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            var nodes = new List<NodeEntity>();
            CreateElementsOnGui<NodeEntity, Ellipse>(nodeList, nodes, Brushes.Blue);

            nodeList = doc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            var switches = new List<SwitchEntity>();
            CreateElementsOnGui<SwitchEntity, Rectangle>(nodeList, switches, Brushes.Green);
        }

        private void CreateElementsOnGui<T1, T2>(XmlNodeList nodeList, List<T1> entityList, Brush fill) where T1 : PowerEntity, new() where T2 : Shape, new()
        {
            foreach (XmlNode item in nodeList)
            {
                double xd = double.Parse(item.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                double yd = double.Parse(item.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);

                uint x = uint.Parse(Math.Round(xd %= ROW_HEIGHT_COUNT).ToString());
                uint y = uint.Parse(Math.Round(yd %= ROW_HEIGHT_COUNT).ToString());

                (x, y) = FindClosestXY(x, y);

                var el = new T2() { Fill = fill };

                Grid.SetRow(el, Convert.ToInt32(x));
                Grid.SetColumn(el, Convert.ToInt32(y));
                grid.Children.Add(el);


                entityList.Add(new T1() { X = x, Y = y });
            }
        }

        private (uint x, uint y) FindClosestXY(uint x, uint y)
        {
            if (!usedCoords.Contains((x, y)))
            {
                usedCoords.Add((x, y));
                return (x, y);
            }

            uint newX = --x;
            newX = (newX == uint.MaxValue) ? ROW_HEIGHT_COUNT : newX;
            uint newY = --y;
            newY = (newY == uint.MaxValue) ? ROW_HEIGHT_COUNT : newY;
            
            while (usedCoords.Contains((newX, newY)))
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (!usedCoords.Contains((newX, newY)))
                        {
                            goto WhileExit;
                        }
                        newY++;
                    }
                    if (!usedCoords.Contains((newX, newY)))
                    {
                        goto WhileExit;
                    }
                    newX++;
                    newY -= 2;
                }

            }

            WhileExit:
            usedCoords.Add((newX, newY));
            return (newX, newY);
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ROW_HEIGHT_COUNT; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(window.Width / ROW_HEIGHT_COUNT) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(window.Height / ROW_HEIGHT_COUNT) });
            }

            LoadXml();
        }
    }
}
