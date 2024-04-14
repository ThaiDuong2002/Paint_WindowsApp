using Contact;
using System.Windows;

namespace PaintProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private Options options = new Options();
        private readonly ShapeSingleton _shapeSingleton = ShapeSingleton.Instance;
        public List<IShape> Shapes = [];
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
        }

        private void drawingHandlerArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void drawingHandlerArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void drawingHandlerArea_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void drawingHandlerArea_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}