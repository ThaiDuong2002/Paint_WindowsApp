using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Contact
{
    public class AdornerShape
    {
        protected const int SIZE = 6;

        public CustomPoint Point { get; set; } = new();
        public CustomPoint CentrePoint { get; set; } = new();

        virtual public string Type { get; set; } = "rotate";

        public AdornerShape()
        {
        }

        virtual public UIElement DrawPoint(double angle,CustomPoint centrePoint)
        {
            UIElement element = new Rectangle()
            {
                Width = SIZE,
                Height = SIZE,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = SIZE / 5,
            };

            Point pos = new() { X = Point.X, Y = Point.Y };
            Point centre = new() { X = CentrePoint.X, Y = CentrePoint.Y };

            Point afterTransform = VectorTransform.Rotate(pos, angle, centre);

            Canvas.SetLeft(element, afterTransform.X - SIZE / 2);
            Canvas.SetTop(element, afterTransform.Y - SIZE / 2);

            return element;
        }
        virtual public bool IsHovering(double angle, double x, double y)
        {
            Point pos = new() { X = Point.X, Y = Point.Y };
            Point centre = new() { X = CentrePoint.X, Y = CentrePoint.Y };

            Point afterTransform = VectorTransform.Rotate(pos, angle, centre);

            return Util.IsBetween(x, afterTransform.X + 15, afterTransform.X - 15)
                && Util.IsBetween(y, afterTransform.Y + 15, afterTransform.Y - 15);
        }

        virtual public string getEdge(double angle)
        {
            string[] edge = { "topleft", "topright", "bottomright", "bottomleft" };
            int index;
            if (Point.X > CentrePoint.X)
                if (Point.Y > CentrePoint.Y)
                    index = 2;
                else
                    index = 1;
            else
                if (Point.Y > CentrePoint.Y)
                index = 3;
            else
                index = 0;

            return edge[index];
        }

        virtual public CustomPoint Handle(double angle, double x, double y)
        {
            CustomPoint result = new()
            {
                //result.X = Math.Cos(angle) * x + Math.Sin(angle) * y;
                //result.Y = Math.Cos(angle) * y + Math.Sin(angle) * x;
                X = x,
                Y = y
            };

            return result;
        }

        virtual public bool IsBeingChosen(string currentChosenType, string currentChosenEdge, double rotateAngle)
        {
            if (currentChosenType == "rotate" || currentChosenType == "move")
                return currentChosenType == Type;

            return currentChosenType == Type && currentChosenEdge == getEdge(rotateAngle);
        }
    }
}
