using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Predmetni_zadatak_2_Grafika.Model;

namespace Predmetni_zadatak_2_Grafika.Services
{
    public static class Common
    {
        private static HashSet<(double, double)> usedCoords = new HashSet<(double, double)>();

        public static double ConvertToCanvas(double point, double scale, double start, double size, double width)
        {
            return Math.Round((point - start) * scale / size) * size % width;
        }

        public static void AddEntities(List<SubstationEntity> entites, XmlNodeList nodeList)
        {
            foreach (XmlNode item in nodeList)
            {
                ToLatLon(double.Parse(item.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture), double.Parse(item.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture), 34, out double x, out double y);
                entites.Add(new SubstationEntity()
                {
                    Id = long.Parse(item.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture),
                    Name = item.SelectSingleNode("Name").InnerText,
                    X = x,
                    Y = y
                });
            }
        }

        public static void AddEntities(List<NodeEntity> entites, XmlNodeList nodeList)
        {
            foreach (XmlNode item in nodeList)
            {
                ToLatLon(double.Parse(item.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture), double.Parse(item.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture), 34, out double x, out double y);
                entites.Add(new NodeEntity()
                {
                    Id = long.Parse(item.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture),
                    Name = item.SelectSingleNode("Name").InnerText,
                    X = x,
                    Y = y
                });
            }
        }

        public static void AddEntities(List<SwitchEntity> entites, XmlNodeList nodeList)
        {
            foreach (XmlNode item in nodeList)
            {
                ToLatLon(double.Parse(item.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture), double.Parse(item.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture), 34, out double x, out double y);
                entites.Add(new SwitchEntity()
                {
                    Id = long.Parse(item.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture),
                    Name = item.SelectSingleNode("Name").InnerText,
                    Status = item.SelectSingleNode("Status").InnerText,
                    X = x,
                    Y = y
                });
            }
        }

        public static void AddEntities(List<LineEntity> entites, XmlNodeList nodeList)
        {
            foreach (XmlNode item in nodeList)
            {
                entites.Add(new LineEntity()
                {
                    Id = long.Parse(item.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture),
                    Name = item.SelectSingleNode("Name").InnerText,
                    ConductorMaterial = item.SelectSingleNode("ConductorMaterial").InnerText,
                    LineType = item.SelectSingleNode("LineType").InnerText,
                    IsUnderground = bool.Parse(item.SelectSingleNode("IsUnderground").InnerText),
                    R = float.Parse(item.SelectSingleNode("R").InnerText, CultureInfo.InvariantCulture),
                    ThermalConstantHeat = long.Parse(item.SelectSingleNode("ThermalConstantHeat").InnerText, CultureInfo.InvariantCulture),
                    FirstEnd = long.Parse(item.SelectSingleNode("FirstEnd").InnerText, CultureInfo.InvariantCulture),
                    SecondEnd = long.Parse(item.SelectSingleNode("SecondEnd").InnerText, CultureInfo.InvariantCulture)
                });
            }
        }

        public static (double x, double y) FindClosestXY(double x, double y, double size)
        {
            if (!usedCoords.Contains((x, y)))
            {
                usedCoords.Add((x, y));
                return (x, y);
            }

            double newX = x - size;
            double newY = y - size;

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
                        newY += size;
                    }
                    if (!usedCoords.Contains((newX, newY)))
                    {
                        goto WhileExit;
                    }
                    newX += size;
                    newY -= 2 * size;
                }

            }

            WhileExit:
            usedCoords.Add((newX, newY));
            return (newX, newY);
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            double diflat = -0.00066286966871111111111111111111111111;
            double diflon = -0.0003868060578;

            int zone = zoneUTM;
            double c_sa = 6378137.000000;
            double c_sb = 6356752.314245;
            double e2 = Math.Pow(Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2), 0.5) / c_sb;
            double e2cuadrada = Math.Pow(e2, 2);
            double c = Math.Pow(c_sa, 2) / c_sb;
            double x = utmX - 500000;
            double y = isNorthHemisphere ? utmY : utmY - 10000000;

            double s = (zone * 6.0) - 183.0;
            double lat = y / (c_sa * 0.9996);
            double v = c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5) * 0.9996;
            double a = x / v;
            double a1 = Math.Sin(2 * lat);
            double a2 = a1 * Math.Pow(Math.Cos(lat), 2);
            double j2 = lat + (a1 / 2.0);
            double j4 = ((3 * j2) + a2) / 4.0;
            double j6 = ((5 * j4) + Math.Pow(a2 * Math.Cos(lat), 2)) / 3.0;
            double alfa = 3.0 / 4.0 * e2cuadrada;
            double beta = 5.0 / 3.0 * Math.Pow(alfa, 2);
            double gama = 35.0 / 27.0 * Math.Pow(alfa, 3);
            double bm = 0.9996 * c * (lat - (alfa * j2) + (beta * j4) - (gama * j6));
            double b = (y - bm) / v;
            double epsi = e2cuadrada * Math.Pow(a, 2) / 2.0 * Math.Pow(Math.Cos(lat), 2);
            double eps = a * (1 - (epsi / 3.0));
            double nab = (b * (1 - epsi)) + lat;
            double senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            double delt = Math.Atan(senoheps / Math.Cos(nab));
            double tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = (delt * (180.0 / Math.PI)) + s + diflon;
            latitude = ((lat + ((1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)) - (3.0 / 2.0 * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat))) * (tao - lat))) * (180.0 / Math.PI)) + diflat;
        }
    }
}
