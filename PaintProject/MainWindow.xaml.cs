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

        #region Singleton, Factory
        public List<IShape> DrawingShapes = new List<IShape>();
        private readonly ShapeSingleton _shapeSingleton = ShapeSingleton.Instance;
        #endregion

        #region App Modes
        public bool isDrawing { get; set; } = false;
        public bool isEditMode { get; set; } = false;
        public bool isSaved { get; set; } = false;
        #endregion

        #region Shape's Properties
        private SolidColorBrush colorSelected = Brushes.Black;
        private DoubleCollection outlineSelected = new DoubleCollection();
        private int sizeSelected = 1;
        IShape currentShape { get; set; } = new MyEllipse();
        string currentShapeName { get; set; } = string.Empty;
        #endregion

        #region Local Store for Copy and Duplicate
        public List<IShape> Shapes = new List<IShape>();
        public List<IShape> _shapes = new List<IShape>();
        Stack<IShape> Buffer { get; set; } = new Stack<IShape>();
        public List<IShape> CopyBuffers { get; set; } = [];
        #endregion

        #region Edit Mode purposes: Copy, Paste, Cut, Duplicate
        public List<AdornerShape> CtrlPoint { get; set; } = new List<AdornerShape>();
        public string SelectedCtrlPointEdge { get; set; } = string.Empty;
        public string SelectedCtrlPointType { get; set; } = string.Empty;
        public double PreviousEditedX { get; set; } = -1;
        public double PreviousEditedY { get; set; } = -1;
        #endregion

        #region For Canvas
        public string BackgroundImage { get; set; } = string.Empty;
        #endregion

        #region Others
        private Options options = new Options();
        #endregion

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

                // Ctrl + M == Change mode
                else if (Keyboard.IsKeyDown(Key.M))
                {
                    changeMode_Click(sender, e);
                }

                // Ctrl + X == Cut
                else if (Keyboard.IsKeyDown(Key.X))
                {
                    cutButton_Click(sender, e);
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
        private void RegisterMouseEvents(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed && isEditMode)
            {
                clearShapes();
            }
        }
        private void RibbonWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DrawingShapes = (List<IShape>)_shapeSingleton.Shapes;
            if (DrawingShapes.Count == 0)
            {
                MessageBox.Show("No shapes found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            sizeComboBox.ItemsSource = options.Sizes;
            sizeComboBox.SelectedIndex = 0;
            outlineComboBox.ItemsSource = options.Outlines;
            outlineComboBox.SelectedIndex = 0;
            solidColorsListView.ItemsSource = options.Colors;
            solidColorsListView.SelectedIndex = 0;
            shapeListView.ItemsSource = DrawingShapes;
            shapeListView.SelectedIndex = 0;

            UpdateCurrentShape(DrawingShapes.First());

            UpdateModeUI();
            KeyDown += RegisterKeyBoardShortCuts;
            MouseDown += RegisterMouseEvents;
        }
        private void UpdateCurrentShape(IShape? shape = null)
        {
            if (shape is not null)
            {
                currentShapeName = shape.Name;
                shapeText.Text = shape.Name;
                currentShape = shape;
            }

            currentShape.Color = colorSelected;
            currentShape.Outline = outlineSelected;
            currentShape.Size = sizeSelected;
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
            builder.Append(serializedShapes).Append('\n').Append($"{BackgroundImage}");
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
                List<IShape> savedShapes = JsonConvert.DeserializeObject<List<IShape>>(shapes, settings)!;

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
                addBackground(filePath);
            }

        }
        private void exportImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp | TIFF (*.tiff)|*.tiff"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                string extension = path[(path.LastIndexOf('\\') + 1)..].Split('.')[1];

                RenderTargetBitmap renderBitmap = new((int)drawingArea.ActualWidth, (int)drawingArea.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);

                drawingArea.Measure(new Size((int)drawingArea.ActualWidth, (int)drawingArea.ActualHeight));
                drawingArea.Arrange(new Rect(new Size((int)drawingArea.ActualWidth, (int)drawingArea.ActualHeight)));

                renderBitmap.Render(drawingArea);

                switch (extension)
                {
                    case "png":
                        PngBitmapEncoder pngEncoder = new();
                        pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                        using (FileStream file = File.Create(path))
                        {
                            pngEncoder.Save(file);
                        }
                        break;
                    case "jpeg":
                        JpegBitmapEncoder jpegEncoder = new();
                        jpegEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                        using (FileStream file = File.Create(path))
                        {
                            jpegEncoder.Save(file);
                        }
                        break;
                    case "tiff":
                        TiffBitmapEncoder tiffEncoder = new();
                        tiffEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                        using (FileStream file = File.Create(path))
                        {
                            tiffEncoder.Save(file);
                        }
                        break;
                    case "bmp":

                        BmpBitmapEncoder bitmapEncoder = new();
                        bitmapEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                        using (FileStream file = File.Create(path))
                        {
                            bitmapEncoder.Save(file);
                        }
                        break;
                    default:
                        break;
                }
            }

            isSaved = true;
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
            if (_shapes.Count > 0 && isEditMode)
            {
                _shapes.ForEach(shape =>
                {
                    Shapes.Remove(shape);
                });

                clearShapes();
            }
            if (!isEditMode && Shapes.Count > 0)
            {
                Shapes.Clear();
                DrawOnCanvas();
            }
        }
        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Buffer.Count == 0)
            {
                return;
            }
            Shapes.Add(Buffer.Pop());
            DrawOnCanvas();
        }
        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Shapes.Count == 0)
            {
                return;
            }
            int lastIndex = Shapes.Count - 1;
            Buffer.Push(Shapes[lastIndex]);
            Shapes.RemoveAt(lastIndex);
            DrawOnCanvas();
        }
        #endregion

        #region Painting: Copy, Paste, Cut, Duplicate
        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                CopyBuffers.Clear();
                _shapes.ForEach(shape =>
                {
                    CopyBuffers.Add(shape);
                });
            }
        }
        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {

            if (isEditMode)
            {
                _shapes.Clear();
                CopyBuffers.ForEach(shape =>
                {


                    ShapePoint shapePoint = (ShapePoint)shape;
                    IShape clone = (IShape)shapePoint.CloneShape();
                    ShapePoint newShapePoint = (ShapePoint)clone;

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
                    Shapes.Add(clone);
                    _shapes.Add(clone);
                });
                DrawOnCanvas();
            }
        }
        private void cutButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                CopyBuffers.Clear();
                _shapes.ForEach(shape =>
                {
                    CopyBuffers.Add(shape);
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

                CopyBuffers.ForEach(shape =>
                {
                    ShapePoint shapePoint = (ShapePoint)shape;
                    IShape clone = (IShape)shapePoint.CloneShape();
                    ShapePoint newShapePoint = (ShapePoint)shape;

                    var pos = Mouse.GetPosition(drawingArea);

                    newShapePoint.TopLeft.X += 15;
                    newShapePoint.TopLeft.Y += 15;
                    newShapePoint.BottomRight.X += 15;
                    newShapePoint.BottomRight.Y += 15;

                    Shapes.Add(clone);
                    _shapes.Add(clone);
                });
                DrawOnCanvas();
            }
        }
        #endregion

        #region Painting: Shapes
        private void shapeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedShape = (IShape)shapeListView.SelectedItem;
            if (selectedShape is not null)
            {
                UpdateCurrentShape(selectedShape);
            }
        }
        #endregion

        #region Painting: Styles
        private void outlineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOutline = (OutlineOption)outlineComboBox.SelectedItem;
            if (selectedOutline is not null)
            {
                outlineSelected = selectedOutline.Value;
            }
        }
        private void sizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSize = (SizeOption)sizeComboBox.SelectedItem;
            if (selectedSize is not null)
            {
                sizeSelected = selectedSize.Value;
            }
        }
        #endregion

        #region Painting: Colors
        private void solidColorsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedColor = (SolidColorBrush)solidColorsListView.SelectedItem;
            if (selectedColor is not null)
            {
                colorSelected = selectedColor;
            }
        }
        private void editColorButton_Click(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                colorSelected = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }
        #endregion

        #region Painting: Drawing
        private void drawingHandlerArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            if (!isEditMode)
            {
                isDrawing = true;
                currentShape.SetStart(e.GetPosition(drawingArea));

                return;
            }

            Point currentPos = e.GetPosition(drawingArea);


            if (_shapes.Count > 0)
            {
                ShapePoint chosen = (ShapePoint)_shapes[0];
                if (CtrlPoint.Count > 0 && SelectedCtrlPointType == String.Empty && SelectedCtrlPointEdge == String.Empty)
                {
                    for (int i = 0; i < CtrlPoint.Count; i++)
                    {
                        if (CtrlPoint[i].IsHovering(chosen.RotateAngle, currentPos.X, currentPos.Y))
                        {
                            SelectedCtrlPointEdge = CtrlPoint[i].getEdge(chosen.RotateAngle);
                            SelectedCtrlPointType = CtrlPoint[i].Type;
                        }
                    }
                }
            }

        }
        private void drawingHandlerArea_MouseMove(object sender, MouseEventArgs e)
        {
            bool isMouseChange = false;

            if (_shapes.Count == 1)
            {
                ShapePoint shapePoint = (ShapePoint)_shapes[0];
                Point currentPosition = e.GetPosition(drawingArea);
                for (int i = 0; i < CtrlPoint.Count; i++)
                {
                    if (CtrlPoint[i].IsHovering(shapePoint.RotateAngle, currentPosition.X, currentPosition.Y) || CtrlPoint[i].IsBeingChosen(this.SelectedCtrlPointType, this.SelectedCtrlPointEdge, shapePoint.RotateAngle))
                    {
                        switch (CtrlPoint[i].getEdge(shapePoint.RotateAngle))
                        {
                            case "topleft" or "bottomright":
                                Mouse.OverrideCursor = Cursors.SizeNWSE;
                                break;
                            case "topright" or "bottomleft":
                                Mouse.OverrideCursor = Cursors.SizeNESW;
                                break;
                            case "top" or "bottom":
                                Mouse.OverrideCursor = Cursors.SizeNS;
                                break;
                            case "left" or "right":
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
                        Mouse.OverrideCursor = null;
                    }

                }
            }
            if (isEditMode)
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

                if (_shapes.Count > 1)
                {
                    _shapes.ForEach(shape =>
                    {
                        ShapePoint shapePoint = (ShapePoint)shape;

                        shapePoint.TopLeft.X = shapePoint.TopLeft.X + dx;
                        shapePoint.TopLeft.Y = shapePoint.TopLeft.Y + dy;
                        shapePoint.BottomRight.X = shapePoint.BottomRight.X + dx;
                        shapePoint.BottomRight.Y = shapePoint.BottomRight.Y + dy;
                    });
                }
                else
                {
                    ShapePoint chosedShape = (ShapePoint)_shapes[0];
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
                                                    chosedShape.TopLeft.X = newPoint.X;
                                                    chosedShape.TopLeft.Y = newPoint.Y;

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
                                            case "right":
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
                                            case "left":
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
                                            case "top":
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
                                            case "bottom":
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
                }
                PreviousEditedX = currentPos.X;
                PreviousEditedY = currentPos.Y;
                DrawOnCanvas();
                return;
            }
            if (isDrawing)
            {
                Point currentPos = e.GetPosition(drawingArea);
                currentShape.SetEnd(currentPos);

                AddPreview();
                UpdateCurrentShape();
            }
        }
        private void drawingHandlerArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            isDrawing = false;

            if (!isEditMode)
            {
                Point currentPos = e.GetPosition(drawingArea);
                currentShape.SetEnd(currentPos);
                MakePreviewStable();

                currentShape = _shapeSingleton.CreateShape(currentShapeName);

                isSaved = false;
            }
            else
            {
                Point currentPos = e.GetPosition(drawingArea);

                for (int i = Shapes.Count - 1; i >= 0; i--)
                {
                    ShapePoint shapePoint = (ShapePoint)Shapes[i];

                    if (shapePoint.IsHovering(currentPos.X, currentPos.Y))
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (_shapes.Contains(Shapes[i]))
                            {
                                _shapes.Remove(Shapes[i]);
                            }
                            else
                            {
                                _shapes.Add(Shapes[i]);
                            }
                        }
                        else
                        {
                            _shapes.Clear();
                            _shapes.Add(Shapes[i]);
                        }
                        DrawOnCanvas();
                        break;
                    }
                }

                PreviousEditedX = -1;
                PreviousEditedY = -1;

                SelectedCtrlPointEdge = string.Empty;
                SelectedCtrlPointType = string.Empty;
            }
        }
        private void drawingHandlerArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                Point currentPos = e.GetPosition(drawingArea);
                Point point = new Point(currentPos.X - 1, currentPos.Y - 1);
                currentShape.SetEnd(point);

                UpdateCurrentShape();
                Shapes.Add(currentShape);

                MakePreviewStable();

                currentShape = _shapeSingleton.CreateShape(currentShapeName);

                isSaved = false;
            }
        }
        #endregion

        #region Others
        private void DrawOnCanvas()
        {
            drawingArea.Children.Clear();

            foreach (var shape in Shapes)
            {

                var element = shape.Draw(shape.Outline!, shape.Color, shape.Size);

                drawingArea.Children.Add(element);
            }

            if (isEditMode && _shapes.Count > 0)
            {
                _shapes.ForEach(shape =>
                {
                    ShapePoint chosenShape = (ShapePoint)shape;
                    drawingArea.Children.Add(chosenShape.AdornerOutline());

                    if (_shapes.Count == 1)
                    {
                        List<AdornerShape> ctrlPoints = chosenShape.GetAdornerShapes();
                        this.CtrlPoint = ctrlPoints;

                        ctrlPoints.ForEach(K =>
                        {
                            drawingArea.Children.Add(K.DrawPoint(chosenShape.RotateAngle, chosenShape.GetCenter()));
                        });
                    }
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
                builder.Append(serializedShapes).Append('\n').Append($"{BackgroundImage}");
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
            isEditMode = false;

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
                drawingHandler.Cursor = Cursors.Hand;
                drawButton.Visibility = Visibility.Visible;
                toolText.Text = "Select";
            }
            else
            {
                drawingHandler.Cursor = Cursors.Cross;
                selectButton.Visibility = Visibility.Visible;
                toolText.Text = "Drawing";
            }
        }
        private void AddPreview()
        {
            previewArea.Children.Clear();
            previewArea.Children.Add(currentShape.Draw(outlineSelected, colorSelected, sizeSelected));
        }
        private void MakePreviewStable()
        {
            previewArea.Children.Clear();

            Shapes.Add(currentShape);
            drawingArea.Children.Add(currentShape.Draw(outlineSelected, colorSelected, sizeSelected));
        }
        #endregion
    }
}