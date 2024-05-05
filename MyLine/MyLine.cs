
using Contact;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LineShape
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
            this.Outline = outline;
            this.Color = color;
            this.Size = size;
            Line line = new Line();
            line.Stroke = color;
            line.StrokeThickness = size;
            line.StrokeDashArray = outline;
            line.X1 = TopLeft.X;
            line.Y1 = TopLeft.Y;
            line.X2 = BottomRight.X;
            line.Y2 = BottomRight.Y;
            RotateTransform transform = new(this.RotateAngle);

            line.RenderTransform = transform;
            return line;
        }

        public void SetStart(Point point)
        {
            TopLeft = new CustomPoint()
            {
                X = point.X,
                Y = point.Y
            };
        }

        public void SetEnd(Point point)
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
            line.RotateAngle = RotateAngle;

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

        override public List<AdornerShape> GetAdornerShapes()
        {
            List<AdornerShape> AdornerShapes = [];

            AdornerShape TopLeftEndPoint = new EndPoint()
            {
                Point = TopLeft,
                CentrePoint = this.GetCenter(),
                Edge = "topleft"
            };

            AdornerShape BottomRightEndPoint = new EndPoint()
            {
                Point = BottomRight,
                CentrePoint = this.GetCenter(),
                Edge = "bottomright"
            };

            AdornerShape moveAdornerShape = new AdornerShape();
            moveAdornerShape.Point = new CustomPoint() { X = (TopLeft.X + BottomRight.X) / 2, Y = (TopLeft.Y + BottomRight.Y) / 2 };
            moveAdornerShape.Type = "move";
            moveAdornerShape.CentrePoint = this.GetCenter();

            AdornerShapes.Add(TopLeftEndPoint);
            AdornerShapes.Add(BottomRightEndPoint);
            AdornerShapes.Add(moveAdornerShape);


            return AdornerShapes;
        }

        override public UIElement AdornerOutline()
        {
            var left = Math.Min(BottomRight.X, TopLeft.X);
            var top = Math.Min(BottomRight.Y, TopLeft.Y);

            var right = Math.Max(BottomRight.X, TopLeft.X);
            var bottom = Math.Max(BottomRight.Y, TopLeft.Y);

            var width = right - left;
            var height = bottom - top;


            var line = new Line()
            {
                X1 = TopLeft.X,
                Y1 = TopLeft.Y,
                X2 = BottomRight.X,
                Y2 = BottomRight.Y,
                StrokeThickness = 2,
                Stroke = Brushes.Black,
                StrokeDashArray = { 4, 2, 4 }
            };

            //Canvas.SetLeft(line, left);
            //Canvas.SetTop(line, top);

            RotateTransform transform = new RotateTransform(RotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            line.RenderTransform = transform;

            return line;
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
