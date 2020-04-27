using System.Collections.Generic;
using System.Windows.Shapes;

namespace Predmetni_zadatak_2_Grafika.Model
{
    public class LineEqualityComparer : IEqualityComparer<Line>
    {
        public bool Equals(Line x, Line y)
        {
            return x.X1 == y.X1 && x.X2 == y.X2 && x.Y1 == y.Y1 && x.Y2 == y.Y2;
        }

        public int GetHashCode(Line obj)
        {
            return obj.GetHashCode();
        }
    }
}
