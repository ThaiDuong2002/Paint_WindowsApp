using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Contact
{
    public class ShapePoint
    {
        public DoubleCollection? Outline { get; set; } = null;
        public SolidColorBrush Color { get; set; } = Brushes.Black;
        public int Size { get; set; } = 1;
        public CustomPoint BottomRight { get; set; } = new CustomPoint();
        public CustomPoint TopLeft { get; set; } = new CustomPoint();
        public double RotateAngle { get; set; } = 0;
        public virtual CustomPoint GetCenter()
        {
            CustomPoint center = new CustomPoint();
            center.X = (TopLeft.X + BottomRight.X) / 2;
            center.Y = (TopLeft.Y + BottomRight.Y) / 2;
            return center;
        }


        virtual public bool IsHovering(double x, double y)
        {
            return Util.IsBetween(x, this.BottomRight.X, this.TopLeft.X)
                && Util.IsBetween(y, this.BottomRight.Y, this.TopLeft.Y);
        }

        virtual public List<AdornerShape> GetAdornerShapes()
        {
            List<AdornerShape> AdornerShapes = new List<AdornerShape>();

            AdornerShape diagPointTopLeft = new DiagPoint();
            //diagPointTopLeft.setPoint(TopLeft.X, TopLeft.Y);
            diagPointTopLeft.Point = TopLeft;
            diagPointTopLeft.CentrePoint = this.GetCenter();

            AdornerShape diagPointBottomLeft = new DiagPoint();
            //diagPointBottomLeft.setPoint(TopLeft.X, BottomRight.Y);
            diagPointBottomLeft.Point = new CustomPoint() { X = TopLeft.X, Y = BottomRight.Y };
            diagPointBottomLeft.CentrePoint = this.GetCenter();

            AdornerShape diagPointTopRight = new DiagPoint();
            //diagPointTopRight.setPoint(BottomRight.X, TopLeft.Y);
            diagPointTopRight.Point = new CustomPoint() { X = BottomRight.X, Y = TopLeft.Y };
            diagPointTopRight.CentrePoint = this.GetCenter();

            AdornerShape diagPointBottomRight = new DiagPoint();
            //diagPointBottomRight.setPoint(BottomRight.X, BottomRight.Y);
            diagPointBottomRight.Point = BottomRight;
            diagPointBottomRight.CentrePoint = this.GetCenter();

            //one way control Point

            AdornerShape diagPointRight = new OneSidePoint();
            //diagPointRight.setPoint(BottomRight.X, (BottomRight.Y + TopLeft.Y) / 2);
            diagPointRight.Point = new CustomPoint() { X = BottomRight.X, Y = (BottomRight.Y + TopLeft.Y) / 2 };
            diagPointRight.CentrePoint = this.GetCenter();

            AdornerShape diagPointLeft = new OneSidePoint();
            //diagPointLeft.setPoint(TopLeft.X, (BottomRight.Y + TopLeft.Y) / 2);
            diagPointLeft.Point = new CustomPoint() { X = TopLeft.X, Y = (BottomRight.Y + TopLeft.Y) / 2 };
            diagPointLeft.CentrePoint = this.GetCenter();

            AdornerShape diagPointTop = new OneSidePoint();
            //diagPointTop.setPoint((TopLeft.X + BottomRight.X) / 2, TopLeft.Y);
            diagPointTop.Point = new CustomPoint() { X = (TopLeft.X + BottomRight.X) / 2, Y = TopLeft.Y };
            diagPointTop.CentrePoint = this.GetCenter();

            AdornerShape diagPointBottom = new OneSidePoint();
            //diagPointBottom.setPoint((TopLeft.X + BottomRight.X) / 2, BottomRight.Y);
            diagPointBottom.Point = new CustomPoint() { X = (TopLeft.X + BottomRight.X) / 2, Y = BottomRight.Y };
            diagPointBottom.CentrePoint = this.GetCenter();


            AdornerShape angleAdornerShape = new RotatePoint();
            //angleAdornerShape.setPoint((BottomRight.X + TopLeft.X) / 2, Math.Min(BottomRight.Y, TopLeft.Y) - 50);
            angleAdornerShape.Point = new CustomPoint() { X = (BottomRight.X + TopLeft.X) / 2, Y = Math.Min(BottomRight.Y, TopLeft.Y) - 50 };
            angleAdornerShape.CentrePoint = this.GetCenter();

            AdornerShape moveAdornerShape = new AdornerShape();
            //moveAdornerShape.setPoint((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2);
            moveAdornerShape.Point = new CustomPoint() { X = (TopLeft.X + BottomRight.X) / 2, Y = (TopLeft.Y + BottomRight.Y) / 2 };
            moveAdornerShape.Type = "move";
            moveAdornerShape.CentrePoint = this.GetCenter();

            AdornerShapes.Add(diagPointTopLeft);
            AdornerShapes.Add(diagPointTopRight);
            AdornerShapes.Add(diagPointBottomLeft);
            AdornerShapes.Add(diagPointBottomRight);

            AdornerShapes.Add(diagPointRight);
            AdornerShapes.Add(diagPointLeft);
            AdornerShapes.Add(diagPointBottom);
            AdornerShapes.Add(diagPointTop);

            AdornerShapes.Add(angleAdornerShape);
            AdornerShapes.Add(moveAdornerShape);

            return AdornerShapes;
        }

        virtual public UIElement AdornerOutline()
        {
            var left = Math.Min(BottomRight.X, TopLeft.X);
            var top = Math.Min(BottomRight.Y, TopLeft.Y);

            var right = Math.Max(BottomRight.X, TopLeft.X);
            var bottom = Math.Max(BottomRight.Y, TopLeft.Y);

            var width = right - left;
            var height = bottom - top;

            var rect = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                StrokeDashArray = { 2, 1 }
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(RotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            rect.RenderTransform = transform;

            return rect;
        }

        public virtual ShapePoint CloneShape()
        {
            ShapePoint shape = new ShapePoint();
            shape.BottomRight = BottomRight;
            shape.TopLeft = TopLeft;
            shape.RotateAngle = RotateAngle;
            return shape;
        }


    }
}
