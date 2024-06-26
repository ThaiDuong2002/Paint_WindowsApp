
using System.Windows;
using System.Windows.Media;

namespace Contact
{
    public interface IShape
    {
        public DoubleCollection? Outline { get; set; }
        public SolidColorBrush Color { get; set; }
        public int Size { get; set; }
        public string Name { get; }
        public string Icon { get; }
        public UIElement Draw(DoubleCollection outline, SolidColorBrush color, int size);
        public IShape Clone();
        public void SetStart(Point point);
        public void SetEnd(Point point);
        public CustomPoint GetStart();
        public CustomPoint GetEnd();
    }

}
