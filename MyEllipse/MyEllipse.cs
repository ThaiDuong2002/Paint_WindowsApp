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

        public IShape Clone()
        {
            return new MyEllipse();
        }

        public UIElement Draw(DoubleCollection outline, SolidColorBrush color, int size)
        {
            var left = Math.Min(BottomRight.X, TopLeft.X);
            var top = Math.Min(BottomRight.Y, TopLeft.Y);
            var right = Math.Max(BottomRight.X, TopLeft.X);
            var bottom = Math.Max(BottomRight.Y, TopLeft.Y);
            var width = right - left;
            var height = bottom - top;

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
    }
}
