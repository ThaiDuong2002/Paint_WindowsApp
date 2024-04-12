namespace PaintProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private Options options = new Options();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            sizeComboBox.ItemsSource = options.Sizes;
            sizeComboBox.SelectedIndex = 1;
            outlineComboBox.ItemsSource = options.Outlines;
            outlineComboBox.SelectedIndex = 0;
            solidColorsListView.ItemsSource = options.Colors;
            solidColorsListView.SelectedIndex = 0;
        }
    }
}