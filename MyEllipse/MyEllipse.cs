using Contact;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace EllipseShape
{
    public class MyEllipse : ShapePoint, IShape
    {
        public string Name => "Ellipse";
        public string Icon => "Images/Shapes/ellipse.png";

        public IShape Clone()
        {
            return new MyEllipse();
        }

        public override ShapePoint CloneShape()
        {
            MyEllipse ellipse = new MyEllipse()
            {
                TopLeft = TopLeft.CloneShape(),
                BottomRight = BottomRight.CloneShape(),
                RotateAngle = RotateAngle,
                Size = Size,
            };

            if (Outline != null)
            {
                ellipse.Outline = Outline.Clone();
            }

            if (Color != null)
            {
                ellipse.Color = Color.Clone();
            }
            return ellipse;
        }

        public UIElement Draw(DoubleCollection outline, SolidColorBrush color, int size)
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

            var ellipse = new Ellipse()
            {
                Width = width,
                Height = height,
                StrokeThickness = size,
                Stroke = color,
                StrokeDashArray = outline
            };

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);


            RotateTransform transform = new(RotateAngle)
            {
                CenterX = width * 1.0 / 2,
                CenterY = height * 1.0 / 2
            };

            ellipse.RenderTransform = transform;

            return ellipse;
        }

        public void SetEnd(Point point)
        {
            BottomRight = new CustomPoint()
            {
                X = point.X,
                Y = point.Y
            };
        }

        public void SetStart(Point point)
        {
            TopLeft.SetStart(point);
        }

        public CustomPoint GetStart()
        {
            return TopLeft;
        }

        public CustomPoint GetEnd()
        {
            return BottomRight;
        }
    }
}
