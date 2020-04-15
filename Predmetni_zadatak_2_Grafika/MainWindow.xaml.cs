using System;
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

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < ROW_HEIGHT_COUNT; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(window.Width / ROW_HEIGHT_COUNT) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(window.Height / ROW_HEIGHT_COUNT) });
            }

            var element = new Rectangle() { Width = 50, Height = 50, Fill = Brushes.Red };
            grid.Children.Add(element);
            Grid.SetRow(element, 5);
            Grid.SetColumn(element, 9);

            LoadXml();
        }

        private void LoadXml()
        {
            var doc = new XmlDocument();
            doc.Load("Geographic.xml");

            var nodeList = doc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

            foreach (XmlNode item in nodeList)
            {
                var x = double.Parse(item.SelectSingleNode("X").InnerText);
                var y = double.Parse(item.SelectSingleNode("Y").InnerText);

                ConvertToNearestGrid(x, y);
            }
            
            
            
            MessageBox.Show(nodeList.Count.ToString());
        }

        private void ConvertToNearestGrid(double x, double y)
        {
            
        }
    }
}
