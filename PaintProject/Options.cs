using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace PaintProject
{
    public class SizeOption
    {
        public required string Label { get; set; }
        public int Value { get; set; }
        public FontWeight FontWeight { get; set; }
    }
    public class OutlineOption
    {
        public required DoubleCollection Value { get; set; }
    }
    public class Options
    {
        public ObservableCollection<SizeOption> Sizes { get; set; }
        public ObservableCollection<OutlineOption> Outlines { get; set; }
        public ObservableCollection<SolidColorBrush> Colors { get; set; }
        public Options()
        {
            Colors = new ObservableCollection<SolidColorBrush>
            {
                Brushes.Black,
                Brushes.Red,
                Brushes.Orange,
                Brushes.Yellow,
                Brushes.Green,
                Brushes.Blue,
                Brushes.Indigo,
                Brushes.Violet,
                Brushes.White,
                Brushes.Gray,
            };
            Sizes = new ObservableCollection<SizeOption>
               {
                new SizeOption { Label = "1px", Value = 1, FontWeight = FontWeights.Light },
                new SizeOption { Label = "3px", Value = 3, FontWeight = FontWeights.Normal },
                new SizeOption { Label = "5px", Value = 5, FontWeight = FontWeights.Medium },
                new SizeOption { Label = "8px", Value = 8, FontWeight = FontWeights.Bold }
            };
            Outlines = new ObservableCollection<OutlineOption>
            {
                new OutlineOption { Value = new DoubleCollection() },
                new OutlineOption { Value = new DoubleCollection { 1, 1 } },
                new OutlineOption { Value = new DoubleCollection { 1, 6 } },
                new OutlineOption { Value = new DoubleCollection { 6, 1 } },
                new OutlineOption { Value = new DoubleCollection { 4, 1, 1, 1, 1, 1 } },
                new OutlineOption { Value = new DoubleCollection { 5, 5, 1, 5 } },
                new OutlineOption { Value = new DoubleCollection { 1, 2, 4 } },
                new OutlineOption { Value = new DoubleCollection { 4, 2, 4 } },
                new OutlineOption { Value = new DoubleCollection { 4, 2, 1, 2 } },
                new OutlineOption { Value = new DoubleCollection { 4, 2, 1, 2, 1, 2 } },
            };
        }
    }
}
