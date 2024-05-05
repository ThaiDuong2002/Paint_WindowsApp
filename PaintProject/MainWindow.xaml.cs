using Contact;
using EllipseShape;
using Newtonsoft.Json;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PaintProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        #region Properties
        public bool isDrawing { get; set; } = false;
        public bool isEditMode { get; set; } = false;
        public bool isSaved { get; set; } = false;
        private Point startPoint;
        private Point endPoint;
        private SolidColorBrush colorSelected = Brushes.Black;
        private int sizeSelected = 1;
        private DoubleCollection outlineSelected = new DoubleCollection();
        private Options options = new Options();
        private readonly ShapeSingleton _shapeSingleton = ShapeSingleton.Instance;
        public List<IShape> Shapes = new List<IShape>();
        public List<IShape> _shapes = new List<IShape>();
        public List<AdornerShape> CtrlPoint { get; set; } = new List<AdornerShape>();
        public string SelectedCtrlPointEdge { get; set; } = string.Empty;
        public string SelectedCtrlPointType { get; set; } = string.Empty;
        public double PreviousEditedX { get; set; } = -1;
        public double PreviousEditedY { get; set; } = -1;
        public string BackgroundImage { get; set; } = string.Empty;
        IShape currentShape = new MyEllipse();
        public bool isClicked = false;
        ShapePoint chosedShape = new MyEllipse();
        Stack<IShape> Buffer { get; set; } = new Stack<IShape>();
        public IShape CopyBuffer { get; set; }
        #endregion

        #region Main Initialization
        public MainWindow()
        {
            InitializeComponent();
        }
        private void RegisterKeyBoardShortCuts(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                // Ctrl + Z == Undo
                if (Keyboard.IsKeyDown(Key.Z))
                {
                    undoButton_Click(sender, e);
                }
                // Ctrl + Y == Redo
                else if (Keyboard.IsKeyDown(Key.Y))
                {
                    redoButton_Click(sender, e);
                }

                // Ctrl + C == Copy
                else if (Keyboard.IsKeyDown(Key.C))
                {
                    copyButton_Click(sender, e);
                }

                // Ctrl + V == Paste
                else if (Keyboard.IsKeyDown(Key.V))
                {
                    pasteButton_Click(sender, e);
                }

                // Ctrl + S == Save
                else if (Keyboard.IsKeyDown(Key.S))
                {
                    saveFileButton_Click(sender, e);
                }

                // Ctrl + O == Open
                else if (Keyboard.IsKeyDown(Key.O))
                {
                    openFileButton_Click(sender, e);
                }

                // Ctrl + N == New
                else if (Keyboard.IsKeyDown(Key.N))
                {
                    addNewButton_Click(sender, e);
                }

                // Ctrl + E == Export
                else if (Keyboard.IsKeyDown(Key.E))
                {
                    exportImageButton_Click(sender, e);
                }

                // Ctrl + P == Import image
                else if (Keyboard.IsKeyDown(Key.P))
                {
                    importImageButton_Click(sender, e);
                }


            }
            if (Keyboard.IsKeyDown(Key.Delete))
            {
                deleteButton_Click(sender, e);
            }
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


            Mouse.OverrideCursor = Cursors.Cross;
            KeyDown += RegisterKeyBoardShortCuts;
        }
        #endregion

        #region Files: New Drawing
        private void addNewButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(BackgroundImage) && Shapes.Count == 0)
            {
                BackgroundImage = string.Empty;
                drawingArea.Background = new SolidColorBrush(Colors.White);
            }
            if (drawingArea.Children.Count == 0)
            {
                return;
            }
            if (isSaved)
            {
                resetToDefault();
                return;
            }

            saveFileDialog();
            resetToDefault();
        }
        #endregion

        #region Files: Open, Save Drawing
        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            var serializedShapes = JsonConvert.SerializeObject(Shapes, settings);
            StringBuilder builder = new StringBuilder();
            builder.Append(serializedShapes).Append("\n").Append($"{BackgroundImage}");
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, builder.ToString());
                isSaved = true;
            }
        }
        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            bool isCancelled = saveFileDialog() == MessageBoxResult.Cancel;

            if (isCancelled)
            {
                return;
            }

            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] content = File.ReadAllLines(dialog.FileName);
                string shapes = content[0];
                string backgroundImage = "";

                if (content.Length > 1)
                {
                    backgroundImage = content[1];
                }

                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects
                };

                Shapes.Clear();
                _shapes.Clear();
                List<IShape> savedShapes = JsonConvert.DeserializeObject<List<IShape>>(shapes, settings);

                foreach (var shape in savedShapes!)
                {
                    Shapes.Add(shape);
                }

                if (!string.IsNullOrEmpty(backgroundImage))
                {
                    addBackground(backgroundImage);
                }
            }
            DrawOnCanvas();
        }
        #endregion

        #region Files: Export, Import Drawing
        private void importImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(filePath)));
                drawingArea.Background = imageBrush;
            }

        }
        private void exportImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Tạo một Grid với màu nền trắng
            Grid grid = new Grid();
            grid.Background = Brushes.White;

            // Tạo một bản sao của drawingArea và thêm vào Grid
            Canvas canvasCopy = new Canvas();
            canvasCopy.Width = drawingArea.ActualWidth;
            canvasCopy.Height = drawingArea.ActualHeight;

            foreach (var shape in _shapes)
            {
                canvasCopy.Children.Add(shape.Draw(shape.Outline, shape.Color, shape.Size, shape.RotateAngleS));
            }

            grid.Children.Add(canvasCopy);

            // Đảm bảo Grid đã được tải lên trước khi render
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            grid.Arrange(new Rect(grid.DesiredSize));

            // Kiểm tra kích thước hợp lệ trước khi tạo RenderTargetBitmap
            if (grid.ActualWidth > 0 && grid.ActualHeight > 0)
            {
                // Sử dụng grid để render
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(grid);

                // Tiếp tục với mã lưu hình ảnh như bạn đã làm
                PngBitmapEncoder pngImage = new PngBitmapEncoder();
                pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "Image files (*.png)|*.png";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (Stream fileStream = File.Create(saveFileDialog.FileName))
                    {
                        pngImage.Save(fileStream);
                    }
                }
            }
            else
            {
                // Báo lỗi hoặc thông báo rằng không thể tạo hình ảnh do kích thước không hợp lệ
                MessageBox.Show("Cannot export image. Grid size is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Painting: Select, Undo, Redo, Delete
        private void changeMode_Click(object sender, RoutedEventArgs e)
        {
            isEditMode = !isEditMode;
            UpdateModeUI();
            if (!isEditMode)
            {
                clearShapes();
            }

        }
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            //xóa shape cuối cùng
            if (_shapes.Count > 0)
            {
                int lastIndex = _shapes.Count - 1;
                _shapes.RemoveAt(lastIndex);
                DrawOnCanvas();
            }
        }
        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Buffer.Count > 0)
            {
                _shapes.Add(Buffer.Pop());
                DrawOnCanvas();
            }
        }
        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shapes.Count > 0)
            {
                int lastIndex = _shapes.Count - 1;
                Buffer.Push(_shapes[lastIndex]);
                _shapes.RemoveAt(lastIndex);
                DrawOnCanvas();
            }

        }
        #endregion

        #region Painting: Copy, Paste, Cut, Duplicate
        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(isEditMode.ToString());
            if (isEditMode)
            {
                _shapes.ForEach(shape =>
                {
                    CopyBuffer = (IShape)shape.Clone();
                });
            }
        }
        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {

            if (isEditMode)
            {
                _shapes.Clear();

                ShapePoint shapePoint = (ShapePoint)CopyBuffer;
                IShape shape = (IShape)shapePoint.CloneShape();
                ShapePoint newShapePoint = (ShapePoint)shape;

                var pos = Mouse.GetPosition(drawingArea);

                if (newShapePoint.IsHovering(pos.X, pos.Y))
                {
                    newShapePoint.TopLeft.X += 15;
                    newShapePoint.TopLeft.Y += 15;
                    newShapePoint.BottomRight.X += 15;
                    newShapePoint.BottomRight.Y += 15;
                }
                else
                {
                    var width = newShapePoint.BottomRight.X - newShapePoint.TopLeft.X;
                    var height = newShapePoint.BottomRight.Y - newShapePoint.TopLeft.Y;

                    newShapePoint.TopLeft.X = pos.X;
                    newShapePoint.TopLeft.Y = pos.Y;
                    newShapePoint.BottomRight.X = pos.X + width;
                    newShapePoint.BottomRight.Y = pos.Y + height;
                }
                Shapes.Add(shape);
                _shapes.Add(shape);
            }
            DrawOnCanvas();
        }
        private void cutButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                _shapes.ForEach(shape =>
                {
                    CopyBuffer = (IShape)shape.Clone();
                });
                _shapes.ForEach(shape =>
                {
                    Shapes.Remove(shape);
                });
                clearShapes();
            }
        }
        private void duplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                copyButton_Click(sender, e);
                _shapes.Clear();

                ShapePoint shapePoint = (ShapePoint)CopyBuffer;
                IShape shape = (IShape)shapePoint.CloneShape();
                ShapePoint newShapePoint = (ShapePoint)shape;

                var pos = Mouse.GetPosition(drawingArea);

                newShapePoint.TopLeft.X += 15;
                newShapePoint.TopLeft.Y += 15;
                newShapePoint.BottomRight.X += 15;
                newShapePoint.BottomRight.Y += 15;

                Shapes.Add(shape);
                _shapes.Add(shape);
            }
            DrawOnCanvas();
        }
        #endregion

        #region Painting: Shapes
        private void shapeListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedShape = (IShape)shapeListView.SelectedItem;
            if (selectedShape is not null)
            {
                currentShape = (IShape)selectedShape.Clone();
            }
        }
        #endregion

        #region Painting: Styles
        private void outlineComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedOutline = (OutlineOption)outlineComboBox.SelectedItem;
            if (selectedOutline is not null)
            {
                outlineSelected = selectedOutline.Value;
            }

            if (_shapes.Count > 0)
            {
                var element = _shapes[_shapes.Count - 1].Draw(outlineSelected, colorSelected, sizeSelected, chosedShape.RotateAngle);
                drawingArea.Children.Add(element);
            }


        }
        private void sizeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedSize = (SizeOption)sizeComboBox.SelectedItem;
            if (selectedSize is not null)
            {
                sizeSelected = selectedSize.Value;
            }
            if (_shapes.Count > 0)
            {
                var element = _shapes[_shapes.Count - 1].Draw(outlineSelected, colorSelected, sizeSelected, chosedShape.RotateAngle);
                drawingArea.Children.Add(element);
            }

        }
        #endregion

        #region Painting: Colors
        private void solidColorsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedColor = (SolidColorBrush)solidColorsListView.SelectedItem;
            if (selectedColor is not null)
            {
                colorSelected = selectedColor;
            }
            if (_shapes.Count > 0)
            {
                var element = _shapes[_shapes.Count - 1].Draw(outlineSelected, colorSelected, sizeSelected, chosedShape.RotateAngle);
                drawingArea.Children.Add(element);
            }

        }
        private void editColorButton_Click(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                colorSelected = new SolidColorBrush(System.Windows.Media.Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }
        #endregion

        #region Painting: Drawing
        private void drawingHandlerArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            if (Mouse.OverrideCursor == Cursors.Cross)
            {
                isDrawing = true;
                startPoint = e.GetPosition(drawingArea);
            }

            Point currentPos = e.GetPosition(drawingArea);


            if (_shapes.Count > 0)
            {
                if (CtrlPoint.Count > 0 && SelectedCtrlPointType == String.Empty && SelectedCtrlPointEdge == String.Empty)
                {
                    for (int i = 0; i < CtrlPoint.Count; i++)
                    {
                        if (CtrlPoint[i].IsHovering(chosedShape.RotateAngle, currentPos.X, currentPos.Y))
                        {
                            SelectedCtrlPointEdge = CtrlPoint[i].getEdge(chosedShape.RotateAngle);
                            SelectedCtrlPointType = CtrlPoint[i].Type;
                        }
                    }
                }
            }

        }
        private void drawingHandlerArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bool isMouseChange = false;

            if (isDrawing && currentShape != null)
            {
                endPoint = e.GetPosition(drawingArea);

                // Xóa tất cả các hình đã vẽ trước đó khỏi drawingArea
                drawingArea.Children.Clear();
                foreach (var shape in _shapes)
                {
                    drawingArea.Children.Add(shape.Draw(shape.Outline, shape.Color, shape.Size, shape.RotateAngleS));
                }
                currentShape.SetStart(startPoint);
                currentShape.SetEnd(endPoint);
                drawingArea.Children.Add(currentShape.Draw(outlineSelected, colorSelected, sizeSelected, 0));
                chosedShape.RotateAngle = 0;
                return;
            }

            Point currentPosition = e.GetPosition(drawingArea);
            for (int i = 0; i < CtrlPoint.Count; i++)
            {
                if (CtrlPoint[i].IsHovering(chosedShape.RotateAngle, currentPosition.X, currentPosition.Y) || CtrlPoint[i].IsBeingChosen(this.SelectedCtrlPointType, this.SelectedCtrlPointEdge, chosedShape.RotateAngle))
                {
                    switch (CtrlPoint[i].getEdge(chosedShape.RotateAngle))
                    {
                        case "topleft":
                        case "bottomright":
                            Mouse.OverrideCursor = Cursors.SizeNWSE;
                            break;
                        case "topright":
                        case "bottomleft":
                            Mouse.OverrideCursor = Cursors.SizeNESW;
                            break;
                        case "top":
                        case "bottom":
                            Mouse.OverrideCursor = Cursors.SizeNS;
                            break;
                        case "left":
                        case "right":
                            Mouse.OverrideCursor = Cursors.SizeWE;
                            break;
                        default:
                            Mouse.OverrideCursor = Cursors.Arrow;
                            break;
                    }

                    if (CtrlPoint[i].Type == "move")
                        Mouse.OverrideCursor = Cursors.SizeAll;
                    else if (CtrlPoint[i].Type == "rotate")
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }

                    isMouseChange = true;
                    break;
                }
                if (!isMouseChange)
                {
                    Mouse.OverrideCursor = Cursors.Cross;
                }

            }
            if (Mouse.OverrideCursor == Cursors.SizeNWSE || Mouse.OverrideCursor == Cursors.SizeNESW || Mouse.OverrideCursor == Cursors.SizeNS || Mouse.OverrideCursor == Cursors.SizeWE || Mouse.OverrideCursor == Cursors.SizeAll || Mouse.OverrideCursor == Cursors.Hand)
            {

                if (_shapes.Count < 1 || (Mouse.LeftButton != MouseButtonState.Pressed))
                {
                    return;
                }

                Point currentPos = e.GetPosition(drawingArea);

                double dx, dy;

                if (PreviousEditedX == -1 || PreviousEditedY == -1)
                {
                    PreviousEditedX = currentPos.X;
                    PreviousEditedY = currentPos.Y;
                    return;
                }

                dx = currentPos.X - PreviousEditedX;
                dy = currentPos.Y - PreviousEditedY;



                CtrlPoint.ForEach(ctrlPoint =>
                {
                    if (ctrlPoint.IsBeingChosen(this.SelectedCtrlPointType, this.SelectedCtrlPointEdge, chosedShape.RotateAngle))
                    {
                        switch (ctrlPoint.Type)
                        {
                            case "rotate":
                                {
                                    CustomPoint centerPoint = chosedShape.GetCenter();

                                    Vector2 v = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                    double angle = (MathF.Atan2(v.Y, v.X) * (180f / Math.PI) + 450f) % 360f;

                                    chosedShape.RotateAngle = angle;

                                    break;
                                }

                            case "move":
                                {
                                    chosedShape.TopLeft.X = chosedShape.TopLeft.X + dx;
                                    chosedShape.TopLeft.Y = chosedShape.TopLeft.Y + dy;
                                    chosedShape.BottomRight.X = chosedShape.BottomRight.X + dx;
                                    chosedShape.BottomRight.Y = chosedShape.BottomRight.Y + dy;
                                    break;
                                }

                            case "diag":
                                {
                                    var centerPoint = chosedShape.GetCenter();

                                    switch (ctrlPoint.getEdge(chosedShape.RotateAngle))
                                    {
                                        case "topleft":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint(
                                                  (vector.X * cosa + vector.Y * sina) + centerPoint.X,
                                                  (-vector.X * sina + vector.Y * cosa) + centerPoint.Y
  );

                                                var oldCenterPoint = chosedShape.GetCenter();

                                                // Cập nhật vị trí mới cho LeftTop
                                                chosedShape.BottomRight.X = newPoint.X;
                                                chosedShape.BottomRight.Y = newPoint.Y;

                                                var newCenterPoint = chosedShape.GetCenter();

                                                // Tính toán sự thay đổi vị trí của các điểm cực
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                // Dịch chuyển tất cả các điểm của hình dạng
                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;



                                                break;
                                            }
                                        case "topright":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();


                                                chosedShape.TopLeft.X = newPoint.X;
                                                chosedShape.BottomRight.Y = newPoint.Y;

                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;

                                                break;
                                            }
                                        case "bottomright":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();

                                                chosedShape.BottomRight.X = newPoint.X;
                                                chosedShape.BottomRight.Y = newPoint.Y;

                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;

                                                break;
                                            }
                                        case "bottomleft":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();

                                                chosedShape.BottomRight.X = newPoint.X;
                                                chosedShape.TopLeft.Y = newPoint.Y;


                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;

                                                break;
                                            }
                                        case "right":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();

                                                chosedShape.TopLeft.X = newPoint.X;


                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;

                                                break;
                                            }
                                        case "left":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();

                                                chosedShape.BottomRight.X = newPoint.X;

                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;


                                                break;
                                            }
                                        case "top":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();

                                                chosedShape.BottomRight.Y = newPoint.Y;

                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;

                                                break;
                                            }
                                        case "bottom":
                                            {
                                                var vector = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                                float a = Util.GetAlphaAngleRadian(chosedShape.RotateAngle);
                                                float cosa = (float)Math.Cos(a);
                                                float sina = (float)Math.Sin(a);
                                                CustomPoint newPoint = new CustomPoint((vector.X * cosa + vector.Y * sina) + centerPoint.X, (-vector.X * sina + vector.Y * cosa) + centerPoint.Y);

                                                var oldCenterPoint = chosedShape.GetCenter();
                                                chosedShape.TopLeft.Y = newPoint.Y;


                                                var newCenterPoint = chosedShape.GetCenter();
                                                (double txx, double tyy) = Util.GetCenterPointTranslation(oldCenterPoint, newCenterPoint, chosedShape);

                                                chosedShape.TopLeft.X += txx;
                                                chosedShape.TopLeft.Y += tyy;
                                                chosedShape.BottomRight.X += txx;
                                                chosedShape.BottomRight.Y += tyy;

                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "end":
                                {
                                    if (ctrlPoint.Point.X == chosedShape.TopLeft.X && ctrlPoint.Point.Y == chosedShape.TopLeft.Y)
                                    {
                                        chosedShape.TopLeft.X = currentPos.X;
                                        chosedShape.TopLeft.Y = currentPos.Y;
                                    }
                                    else if (ctrlPoint.Point.X == chosedShape.BottomRight.X && ctrlPoint.Point.Y == chosedShape.BottomRight.Y)
                                    {
                                        chosedShape.BottomRight.X = currentPos.X;
                                        chosedShape.BottomRight.Y = currentPos.Y;
                                    }
                                    break;
                                }
                        }
                    }

                });
                PreviousEditedX = currentPos.X;
                PreviousEditedY = currentPos.Y;
                DrawOnCanvas();
                return;
            }
        }
        private void drawingHandlerArea_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (currentShape != null && isDrawing)
            {
                isDrawing = false;
                _shapes.Add((IShape)currentShape.Clone());
            }


            // sau khi vẽ xong
            chosedShape = (ShapePoint)currentShape;


            drawingArea.Children.Add(chosedShape.AdornerOutline());

            List<AdornerShape> ctrlPoints = chosedShape.GetAdornerShapes();
            this.CtrlPoint = ctrlPoints;


            foreach (AdornerShape ctrlPoint in ctrlPoints)
            {
                drawingArea.Children.Add(ctrlPoint.DrawPoint(chosedShape.RotateAngle, chosedShape.GetCenter()));
            }




            if (Mouse.OverrideCursor != Cursors.Cross)
            {

                this.PreviousEditedX = -1;
                this.PreviousEditedY = -1;

                this.SelectedCtrlPointEdge = String.Empty;
                this.SelectedCtrlPointType = String.Empty;
            }
        }
        #endregion

        #region Others
        private void DrawOnCanvas()
        {
            drawingArea.Children.Clear();

            foreach (var shape in _shapes)
            {

                var element = shape.Draw(shape.Outline, shape.Color, shape.Size, shape.RotateAngleS);
                if (_shapes.Count == 1 || shape == _shapes[_shapes.Count - 1])
                {
                    element = shape.Draw(outlineSelected, colorSelected, sizeSelected, chosedShape.RotateAngle);
                }


                drawingArea.Children.Add(element);
            }

            if (Mouse.OverrideCursor != Cursors.Cross && _shapes.Count > 0)
            {
                drawingArea.Children.Add(chosedShape.AdornerOutline());

                List<AdornerShape> ctrlPoints = chosedShape.GetAdornerShapes();
                this.CtrlPoint = ctrlPoints;

                ctrlPoints.ForEach(K =>
                {
                    drawingArea.Children.Add(K.DrawPoint(chosedShape.RotateAngle, chosedShape.GetCenter()));
                });


            }
        }
        private void addBackground(string path)
        {
            BackgroundImage = path;
            drawingArea.Background = new ImageBrush()
            {
                ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute)),
                Stretch = Stretch.UniformToFill
            };
        }
        private MessageBoxResult saveFileDialog()
        {
            if (isSaved || drawingArea.Children.Count == 0)
            {
                return MessageBoxResult.Yes;
            }

            var result = MessageBox.Show("Do you want to save the current drawing?", "Save Drawing", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects
                };
                var serializedShapes = JsonConvert.SerializeObject(Shapes, settings);

                StringBuilder builder = new StringBuilder();
                builder.Append(serializedShapes).Append("\n").Append($"{BackgroundImage}");
                var dialog = new System.Windows.Forms.SaveFileDialog
                {
                    Filter = "JSON (*.json)|*.json"
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, builder.ToString());
                }
                isSaved = true;
            }
            return result;
        }
        private void resetToDefault()
        {
            isSaved = false;
            isDrawing = false;

            clearDrawingArea();
        }
        private void clearDrawingArea()
        {
            BackgroundImage = string.Empty;

            Shapes.Clear();
            _shapes.Clear();
            drawingArea.Children.Clear();
            drawingArea.Background = new SolidColorBrush(Colors.White);
        }
        private void clearShapes()
        {
            _shapes.Clear();
            DrawOnCanvas();
        }
        private void UpdateModeUI()
        {
            drawButton.Visibility = Visibility.Collapsed;
            selectButton.Visibility = Visibility.Collapsed;


            if (isEditMode)
            {
                drawingArea.Cursor = Cursors.Hand;
                drawButton.Visibility = Visibility.Visible;
                toolText.Text = "Select";
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Cross;
                selectButton.Visibility = Visibility.Visible;
                toolText.Text = "Drawing";
            }
        }
        #endregion
    }
}