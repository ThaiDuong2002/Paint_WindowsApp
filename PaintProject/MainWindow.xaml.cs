using Contact;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LineShape;
using RectangleShape;
using EllipseShape;

namespace PaintProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        bool isDrawing = false;
        private Point startPoint;
        private Point endPoint;

        private SolidColorBrush colorSelected= Brushes.Black;
        private int sizeSelected = 1;
        private DoubleCollection outlineSelected = new DoubleCollection();

        private Options options = new Options();
        private readonly ShapeSingleton _shapeSingleton = ShapeSingleton.Instance;
        public List<IShape> Shapes = [];
        public List<IShape> _shapes = new List<IShape>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Shapes = (List<IShape>)_shapeSingleton.Shapes;
            if (Shapes.Count == 0)
            {
                MessageBox.Show("No shapes found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
           

            sizeComboBox.ItemsSource = options.Sizes;
            sizeComboBox.SelectedIndex = 0;
            outlineComboBox.ItemsSource = options.Outlines;
            outlineComboBox.SelectedIndex = 0;
            solidColorsListView.ItemsSource = options.Colors;
            solidColorsListView.SelectedIndex = 0;
            shapeListView.ItemsSource = Shapes;
            shapeListView.SelectedIndex = 0;


            this.Cursor = Cursors.Cross;
        }
        IShape currentShape = new MyEllipse();

        private void drawingHandlerArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            isDrawing = true;
            startPoint = e.GetPosition(drawingArea);
            currentShape.SetStart(startPoint);

        }
        private void drawingHandlerArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                endPoint = e.GetPosition(drawingArea);
                currentShape.SetEnd(endPoint);

                // Xóa tất cả các hình đã vẽ trước đó khỏi drawingArea
                previewDrawingArea.Children.Clear();



                previewDrawingArea.Children.Add(currentShape.Draw(outlineSelected, colorSelected, sizeSelected));
            }
    

        }


        private void drawingHandlerArea_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                previewDrawingArea.Children.Clear();
                drawingArea.Children.Add(currentShape.Draw(outlineSelected, colorSelected, sizeSelected));
            }
        }

        private void ShapeListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedShape = (IShape)shapeListView.SelectedItem;
            if (selectedShape is not null)
            {
                currentShape = selectedShape.Clone();
            }
        }

     
        private void outlineComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedOutline = (OutlineOption)outlineComboBox.SelectedItem;
            if (selectedOutline is not null)
            {
                outlineSelected = selectedOutline.Value;
            }

        }

        private void sizeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedSize = (SizeOption)sizeComboBox.SelectedItem;
            if (selectedSize is not null)
            {
                sizeSelected = selectedSize.Value;
            }

        }

        private void solidColorsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedColor = (SolidColorBrush)solidColorsListView.SelectedItem;
            if (selectedColor is not null)
            {
                colorSelected = selectedColor;
            }

        }

  
    }
}