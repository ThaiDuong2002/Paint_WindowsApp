using Contact;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
namespace EllipseShape
{
    public class MyEllipse : ShapePoint, IShape
    {
        public string Name => "Ellipse";
        public string Icon => "Images/Shapes/ellipse.png";
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
            this.RotateAngleS = RotateAngle;


            RotateTransform transform = new(this.RotateAngleS)
            {
                CenterX = width * 1.0 / 2,
                CenterY = height * 1.0 / 2
            };

            ellipse.RenderTransform = transform;

            return ellipse;
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
