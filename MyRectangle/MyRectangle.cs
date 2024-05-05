
using Contact;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace RectangleShape
{
    public class MyRectangle : ShapePoint, IShape
    {
        public string Name => "Rectangle";
        public string Icon => "Images/Shapes/rectangle.png";
        public double RotateAngleS { get; set; } = 0;


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public UIElement Draw(DoubleCollection outline, SolidColorBrush color, int size, double RotateAngle)
        {
            var left = Math.Min(BottomRight.X, TopLeft.X);
            var top = Math.Min(BottomRight.Y, TopLeft.Y);
            var right = Math.Max(BottomRight.X, TopLeft.X);
            var bottom = Math.Max(BottomRight.Y, TopLeft.Y);
            var width = right - left;
            var height = bottom - top;

            this.Outline = outline;
            this.Color = color;
            this.Size = size;

            var rectangle = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = size,
                Stroke = color,
                StrokeDashArray = outline
            };

            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            this.RotateAngleS = RotateAngle;

            RotateTransform transform = new(RotateAngleS)
            {
                CenterX = width * 1.0 / 2,
                CenterY = height * 1.0 / 2
            };

            rectangle.RenderTransform = transform;

            return rectangle;
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
            MyRectangle temp = new MyRectangle();
            temp.BottomRight = BottomRight;
            temp.TopLeft = TopLeft;
            temp.Size = Size;
            if (Outline != null)
            {
                temp.Outline = this.Outline.Clone();
            }
            if (Color != null)
            {
                temp.Color = this.Color.Clone();
            }
            return temp;
        }

        public CustomPoint GetStart()
        {
            return BottomRight;
        }

        public CustomPoint GetEnd()
        {
            return TopLeft;
        }


    }

}
