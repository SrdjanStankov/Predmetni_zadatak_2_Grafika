namespace Predmetni_zadatak_2_Grafika.Model
{
    public class Vertex
    {
        public Vertex Parent { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public char Data { get; set; }

        public Vertex(int x, int y, char data)
        {
            X = x;
            Y = y;
            Data = data;
        }

        public Vertex(Vertex parent, int x, int y)
        {
            Parent = parent;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
