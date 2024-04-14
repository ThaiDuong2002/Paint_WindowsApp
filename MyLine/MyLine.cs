
using Contact;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyLine
{
    public class MyLine : ShapePoint, IShape
    {
        public string Name => "Line";
        public string Icon => "Images/Shapes/line.png";

        public IShape Clone()
        {
            return new MyLine();
        }

        public UIElement Draw(DoubleCollection outline, SolidColorBrush color, int size)
        {
            Line line = new Line();
            line.Stroke = color;
            line.StrokeThickness = size;
            line.StrokeDashArray = outline;
            line.X1 = TopLeft.X;
            line.Y1 = TopLeft.Y;
            line.X2 = BottomRight.X;
            line.Y2 = BottomRight.Y;
            return line;
        }

        public void SetEnd(Point point)
        {
            TopLeft = new CustomPoint()
            {
                X = point.X,
                Y = point.Y
            };
        }

        public void SetStart(Point point)
        {
            BottomRight = new CustomPoint()
            {
                X = point.X,
                Y = point.Y
            };
        }
        public override ShapePoint CloneShape()
        {
            MyLine line = new MyLine();
            line.BottomRight = BottomRight;
            line.TopLeft = TopLeft;
            line.Size = Size;

            if (Outline != null)
            {
                line.Outline = this.Outline.Clone();
            }
            if (Color != null)
            {
                line.Color = this.Color.Clone();
            }
            return line;
        }
    }

}
