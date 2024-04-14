using System.Windows.Media;

namespace Contact
{
    public class ShapePoint
    {
        public DoubleCollection? Outline { get; set; } = null;
        public SolidColorBrush Color { get; set; } = Brushes.Black;
        public int Size { get; set; } = 1;
        public CustomPoint BottomRight { get; set; } = new CustomPoint();
        public CustomPoint TopLeft { get; set; } = new CustomPoint();
        public virtual CustomPoint GetCenter()
        {
            CustomPoint center = new CustomPoint();
            center.X = (TopLeft.X + BottomRight.X) / 2;
            center.Y = (TopLeft.Y + BottomRight.Y) / 2;
            return center;
        }
        public virtual ShapePoint CloneShape()
        {
            ShapePoint shape = new ShapePoint();
            shape.BottomRight = BottomRight;
            shape.TopLeft = TopLeft;
            return shape;
        }
    }
}
