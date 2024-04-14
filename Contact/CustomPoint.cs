using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contact
{
    public class CustomPoint : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }
        public DoubleCollection? Outline { get; set; } = new DoubleCollection();
        public SolidColorBrush Color { get; set; } = Brushes.Black;
        public int Size { get; set; }
        public string Name => "Point";
        public string Icon => "M 0,0 L 0,0";
        public CustomPoint()
        {
            X = 0;
            Y = 0;
        }

        public IShape Clone()
        {
            CustomPoint point = new CustomPoint();
            point.X = X;
            point.Y = Y;

            return point;
        }
        public CustomPoint CloneShape()
        {
            return new CustomPoint();
        }

        public UIElement Draw(DoubleCollection outline, SolidColorBrush color, int size)
        {
            Line line = new Line();
            line.X1 = X;
            line.Y1 = Y;
            line.X2 = X;
            line.Y2 = Y;
            line.Stroke = color;
            line.StrokeThickness = size;
            line.StrokeDashArray = outline;

            return line;
        }

        public void SetEnd(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public void SetStart(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
    }
}
