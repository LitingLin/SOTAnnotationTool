using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using AnnotationTool.Data;
using AnnotationTool.Data.Model;
using AnnotationTool.NativeInteropServices;

namespace AnnotationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel = new MainWindowViewModel();
        private SelectiveBoxCanvas _selectiveBoxCanvas;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            SetupKeyBindings();
            _selectiveBoxCanvas = new SelectiveBoxCanvas(AnnotationCanvas);
            _selectiveBoxCanvas.OnShapeChanged += SelectiveBoxCanvasOnOnShapeChanged;
            ButtonBrowseDatasetFile.Focusable = false;
            viewModel.IsTrackerActivated = true;
            viewModel.IsUpdateMatlabRecordOnAppropriateOpportunity = true;
        }

        private void SelectiveBoxCanvasOnOnShapeChanged(Rect shape)
        {
            _shapeTextBoxIsModifiedByUser = false;
            viewModel.X = Convert.ToInt32(shape.X);
            viewModel.Y = Convert.ToInt32(shape.Y);
            viewModel.W = Convert.ToInt32(shape.Width);
            viewModel.H = Convert.ToInt32(shape.Height);
            _shapeTextBoxIsModifiedByUser = true;
            DrawCurrentTarget();
            SetMaximumValue();
        }

        private void SetupKeyBindings()
        {
            viewModel.KeyOfFullOcclusion = "1";
            viewModel.KeyOfOutOfView = "2";
            viewModel.KeyPreviousFrame = "箭头上↑";
            viewModel.KeyNextFrame = "箭头下↓";
            viewModel.KeySubmit = "空格";
            viewModel.KeyOfMoveUp = "W";
            viewModel.KeyOfMoveDown = "S";
            viewModel.KeyOfMoveLeft = "A";
            viewModel.KeyOfMoveRight = "D";
            viewModel.KeyOfExpandUpDown = "I";
            viewModel.KeyOfShrinkUpDown = "K";
            viewModel.KeyOfExpandLeftRight = "J";
            viewModel.KeyOfShinkLeftRight = "L";
            viewModel.KeyOfReTrack = "R";
            viewModel.KeyOfCopyLastFrame = "C";
            viewModel.KeyOfDeleteRecord = "退格键←";
            viewModel.KeyOfZoomIn = "+";
            viewModel.KeyOfZoomOut = "-";
        }

        private void TextBoxDatasetFilePath_OnGotFocus(object sender, RoutedEventArgs e)
        {
            ButtonBrowseDatasetFile.IsDefault = true;
        }

        private void TextBoxDatasetFilePath_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ButtonBrowseDatasetFile.IsDefault = false;
        }

        private void Reset()
        {
            _currentIndex = null;
            _lastIndex = null;
            InitialTargetCanvas.Background = null;
            AnnotationCanvas.Background = null;
            CurrentTargetCanvas.Background = null;
            _selectiveBoxCanvas.Clear();
            CurrentTargetCanvas.Children.Clear();
            viewModel.FrameInformations.Clear();
        }

        private void ImportRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "res.mat 文件|res.mat";
            openFileDialog.Multiselect = false;
            bool? isGotResult = openFileDialog.ShowDialog();
            if (!isGotResult.HasValue || !isGotResult.Value)
                return;

            ImportRecord(openFileDialog.FileName);
        }
        private void ImportRecord(string recordPath)
        {
            _annotationRecordDataAccessor.Import(recordPath);

            var currentIndex = _currentIndex;

            Reset();

            InitializeUserInterface();

            OpenFrame(currentIndex.Value);
        }

        private AnnotationRecordDataAccessor _annotationRecordDataAccessor;

        private void InitializeUserInterface()
        {
            int numberOfFrames = _annotationRecordDataAccessor.GetNumberOfRecords();

            for (int index = 0; index < numberOfFrames; ++index)
            {
                var annotationRecord = _annotationRecordDataAccessor.Get(index);
                viewModel.FrameInformations.Add(new MainWindowViewModel.FrameInformation() { Id = index + 1, IsLabeled = annotationRecord.IsLabeled, X = annotationRecord.X, Y = annotationRecord.Y, W = annotationRecord.W, H = annotationRecord.H, IsFullOccluded = annotationRecord.IsFullyOccluded, IsOutOfView = annotationRecord.IsOutOfView });
            }
            var firstRecord = _annotationRecordDataAccessor.Get(0);

            BitmapImage firstFrameImage = new BitmapImage();
            firstFrameImage.BeginInit();
            firstFrameImage.UriSource = new Uri(_annotationRecordDataAccessor.GetImagePath(0));
            firstFrameImage.SourceRect = new Int32Rect(firstRecord.X, firstRecord.Y, firstRecord.W, firstRecord.H);
            firstFrameImage.EndInit();

            InitialTargetCanvas.Background = new ImageBrush(firstFrameImage);
            InitialTargetCanvas.Width = firstRecord.W;
            InitialTargetCanvas.Height = firstRecord.H;
            viewModel.CurrentIndexOfFrame = 0;
            viewModel.TotalNumberOfFrames = numberOfFrames;
            viewModel.MaxIndexOfFrame = numberOfFrames;
            viewModel.CurrentSequenceName = _annotationRecordDataAccessor.GetName();
        }

        private void OpenSequence(string path)
        {
            if (_annotationRecordDataAccessor != null)
            {
                if (viewModel.IsUpdateMatlabRecordOnAppropriateOpportunity)
                    _annotationRecordDataAccessor.UpdateMatlabFile();
                _annotationRecordDataAccessor.Dispose();
                _annotationRecordDataAccessor = null;
            }
#if !DEBUG
            try
            {
#endif
                _annotationRecordDataAccessor = new AnnotationRecordDataAccessor(path);
#if !DEBUG
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
#endif

            InitializeUserInterface();

            OpenFrame(0);
        }

        private int _currentTargetViewContextPercent = 20;
        public static BitmapSource FromArray(byte[] data, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
            wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

            return wbm;
        }

        private DrawCurrentTargetNativeHelper _drawCurrentTargetNativeHelper = null;
        void DrawCurrentTarget()
        {
            int x = viewModel.X;
            int y = viewModel.Y;
            int w = viewModel.W;
            int h = viewModel.H;

            if (w == 0 || h == 0)
            {
                CurrentTargetCanvas.Background = null;
                CurrentTargetCanvas.Children.Clear();
                return;
            }
            PresentationSource source = PresentationSource.FromVisual(CurrentTargetViewbox);
            
            var viewPixelSize = source.CompositionTarget.TransformToDevice.Transform(new Vector(CurrentTargetViewbox.ActualWidth,
                CurrentTargetViewbox.ActualHeight));
            if (viewPixelSize.X <= 0.0 || viewPixelSize.Y <= 0.0)
                return;

            if (_drawCurrentTargetNativeHelper != null)
            {
                var result =
                    _drawCurrentTargetNativeHelper.update(Convert.ToUInt64(Math.Round(viewPixelSize.X)),
                        Convert.ToUInt64(Math.Round(viewPixelSize.Y)),
                        (ulong) x, (ulong) y, (ulong) w, (ulong) h,
                        (double) _currentTargetViewContextPercent / 100.0);

                CurrentTargetCanvas.Children.Clear();
                var currentTargetImage = FromArray(result.image, (int) result.imageWidth, (int) result.imageHeight, 3);

                CurrentTargetCanvas.Width = result.imageWidth;
                CurrentTargetCanvas.Height = (int) result.imageHeight;
                CurrentTargetCanvas.Background = new ImageBrush(currentTargetImage);
                Rectangle rectangle = new Rectangle();
                rectangle.Width = result.width;
                rectangle.Height = result.height;
                rectangle.Stroke = new SolidColorBrush(Colors.Red);
                rectangle.Opacity = 0.4;
                rectangle.StrokeThickness = result.scalingRatio;
                CurrentTargetCanvas.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, result.x);
                Canvas.SetTop(rectangle, result.y);
            }
            else
            {
                double windowElementPixelWidth = viewPixelSize.X;
                double windowElementPixelHeight = viewPixelSize.Y;
                int boundingBoxX = x;
                int boundingBoxY = y;
                int boundingBoxWidth = w;
                int boundingBoxHeight = h;


                double cropX, cropY, cropWidth, cropHeight;

                double context = (double)_currentTargetViewContextPercent / 100.0;
                double boundingBoxContextSizeX = Math.Round((double)boundingBoxWidth * context);
                double boundingBoxContextSizeY = Math.Round(((double)boundingBoxHeight * context));

                double boundingBoxWithContextWidth = (double)boundingBoxWidth + 2.0 * boundingBoxContextSizeX;
                double boundingBoxWithContextHeight = (double)boundingBoxHeight + 2.0 * boundingBoxContextSizeY;

                double windowElementXYRatio = (double)windowElementPixelWidth / (double)windowElementPixelHeight;

                if (windowElementPixelWidth / boundingBoxWithContextWidth > windowElementPixelHeight / boundingBoxWithContextHeight)
                {
                    cropWidth = Math.Round(boundingBoxWithContextHeight * windowElementXYRatio);
                    cropHeight = boundingBoxWithContextHeight;

                    boundingBoxContextSizeX = Math.Round((cropWidth - (double)boundingBoxWidth) / 2.0);
                }
                else
                {
                    cropWidth = boundingBoxWithContextWidth;
                    cropHeight = Math.Round(boundingBoxWithContextWidth / windowElementXYRatio);

                    boundingBoxContextSizeY = Math.Round((cropHeight - (double)boundingBoxHeight) / 2.0);
                }

                cropX = (double)boundingBoxX - boundingBoxContextSizeX;
                cropY = (double)boundingBoxY - boundingBoxContextSizeY;

                int quantifiedCropWidth = (int)cropWidth;
                int quantifiedCropHeight = (int)cropHeight;

                int quantifiedContextWidth = (int)((double)boundingBoxX - cropX);
                int quantifiedContextHeight = (int)((double)boundingBoxY - cropY);

                int sourceCropX, sourceCropY, sourceCropWidth, sourceCropHeight;
                int destinationCropX, destinationCropY;

                int sourceImageWidth = _currentImage.PixelWidth;
                int sourceImageHeight = _currentImage.PixelHeight;

                if (cropX < 0)
                {
                    sourceCropX = 0;
                    destinationCropX = (int)(-cropX);
                }
                else
                {
                    sourceCropX = (int)cropX;
                    destinationCropX = 0;
                }

                if (cropY < 0)
                {
                    sourceCropY = 0;
                    destinationCropY = (int)(-cropY);
                }
                else
                {
                    sourceCropY = (int)cropY;
                    destinationCropY = 0;
                }

                if (cropX + cropWidth > sourceImageWidth)
                {
                    sourceCropWidth = sourceImageWidth - sourceCropX;
                }
                else
                {
                    sourceCropWidth = (int)(cropX + cropWidth - sourceCropX);
                }
                if (cropY + cropHeight > sourceImageHeight)
                {
                    sourceCropHeight = sourceImageHeight - sourceCropY;
                }
                else
                {
                    sourceCropHeight = (int)(cropY + cropHeight - sourceCropY);
                }

                WriteableBitmap writableBitmap = new WriteableBitmap(quantifiedCropWidth, quantifiedCropHeight, _currentImage.DpiX, _currentImage.DpiY, _currentImage.Format, _currentImage.Palette);

                // Stride = (width) x (bytes per pixel)
                int stride = sourceCropWidth * (_currentImage.Format.BitsPerPixel / 8);
                byte[] pixels = new byte[sourceCropHeight * stride];
                _currentImage.CopyPixels(new Int32Rect(sourceCropX, sourceCropY, sourceCropWidth, sourceCropHeight), pixels, stride, 0);
                writableBitmap.WritePixels(new Int32Rect(destinationCropX, destinationCropY, sourceCropWidth, sourceCropHeight), pixels, stride, 0);


                CurrentTargetCanvas.Children.Clear();

                CurrentTargetCanvas.Width = quantifiedCropWidth;
                CurrentTargetCanvas.Height = quantifiedCropHeight;
                CurrentTargetCanvas.Background = new ImageBrush(writableBitmap);
                Rectangle rectangle = new Rectangle();
                rectangle.Width = boundingBoxWidth;
                rectangle.Height = boundingBoxHeight;
                rectangle.Stroke = new SolidColorBrush(Colors.Red);
                rectangle.Opacity = 0.4;
                rectangle.StrokeThickness = 1;
                CurrentTargetCanvas.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, quantifiedContextWidth);
                Canvas.SetTop(rectangle, quantifiedContextHeight);
            }

        }

        private byte[] _bgrRawPixelCache = null;
        private int _bgrRawPixelCacheSize = 0;
        private BitmapImage _lastDecodedImage = null;
        private byte[] DecodeImage(BitmapImage image)
        {
            if (_lastDecodedImage == image)
                return _bgrRawPixelCache;

            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(image, PixelFormats.Bgr24, null, 0);
            int bytesPerPixel = (convertedBitmap.Format.BitsPerPixel + 7) / 8;
            int requiredCacheSize = bytesPerPixel * convertedBitmap.PixelWidth * convertedBitmap.PixelHeight;
            if (_bgrRawPixelCache == null || _bgrRawPixelCacheSize != requiredCacheSize)
            {
                _bgrRawPixelCache = new byte[bytesPerPixel * convertedBitmap.PixelWidth * convertedBitmap.PixelHeight];
                _bgrRawPixelCacheSize = requiredCacheSize;
            }

            Int32Rect rect = new Int32Rect(0, 0, convertedBitmap.PixelWidth, convertedBitmap.PixelHeight);
            convertedBitmap.CopyPixels(rect, _bgrRawPixelCache, bytesPerPixel * convertedBitmap.PixelWidth, 0);

            _lastDecodedImage = image;

            return _bgrRawPixelCache;
        }

        bool TrackTargetBasedOnPreviousFrame()
        {
            if (!_currentIndex.HasValue)
                return false;
            if (_currentIndex.Value == 0)
                return false;
            int currentIndex = _currentIndex.Value;
            if (!_lastIndex.HasValue || _lastIndex.Value != currentIndex - 1)
            {
                _lastImage = new BitmapImage(new Uri(_annotationRecordDataAccessor.GetImagePath(currentIndex - 1)));
                _lastIndex = currentIndex - 1;
            }

            var lastRecord = _annotationRecordDataAccessor.Get(currentIndex - 1);

            if (!lastRecord.IsLabeled || lastRecord.IsFullyOccluded || lastRecord.IsOutOfView)
                return false;

            Rect predicted;

            using (BACFTracker tracker = new BACFTracker())
            {
                tracker.Initialize(DecodeImage(_lastImage), _lastImage.PixelWidth, _lastImage.PixelHeight, lastRecord.X, lastRecord.Y, lastRecord.W, lastRecord.H);
                var bbox = tracker.Predict(DecodeImage(_currentImage), _currentImage.PixelWidth, _currentImage.PixelHeight);
                predicted = new Rect();
                predicted.X = bbox.Item1;
                predicted.Y = bbox.Item2;
                predicted.Width = bbox.Item3;
                predicted.Height = bbox.Item4;
            }

            if (predicted.X >= _currentImage.PixelWidth || predicted.Y >= _currentImage.PixelHeight)
            {
                predicted.X = lastRecord.X;
                predicted.Y = lastRecord.Y;
                predicted.Width = lastRecord.W;
                predicted.Height = lastRecord.H;
            }
            else
            {
                if (predicted.X < 0)
                    predicted.X = 0;
                if (predicted.Y < 0)
                    predicted.Y = 0;
                if (predicted.X + predicted.Width > _currentImage.PixelWidth)
                    predicted.Width = _currentImage.PixelWidth - predicted.X;
                if (predicted.Y + predicted.Height > _currentImage.PixelHeight)
                    predicted.Height = _currentImage.PixelHeight - predicted.Y;
            }

            _selectiveBoxCanvas.Create(predicted);

            _shapeTextBoxIsModifiedByUser = false;
            viewModel.X = Convert.ToInt32(Math.Round(predicted.X));
            viewModel.Y = Convert.ToInt32(Math.Round(predicted.Y));
            viewModel.W = Convert.ToInt32(Math.Round(predicted.Width));
            viewModel.H = Convert.ToInt32(Math.Round(predicted.Height));
            _shapeTextBoxIsModifiedByUser = true;

            viewModel.IsFullyOccluded = false;
            viewModel.IsOutOfView = false;

            return true;
        }

        private BitmapImage _currentImage;
        private int? _currentIndex;
        private BitmapImage _lastImage;
        private int? _lastIndex;

        private bool _isUserSelectFrameInformationDataGridRow = true;

        private void SetBoundingBox(AnnotationRecord record)
        {
            if (record.W > 0 && record.H > 0)
            {
                _shapeTextBoxIsModifiedByUser = false;
                viewModel.X = record.X;
                viewModel.Y = record.Y;
                viewModel.W = record.W;
                viewModel.H = record.H;
                _shapeTextBoxIsModifiedByUser = true;
            }
            else
            {
                if (_currentIndex > 0)
                {
                    var lastFrameRecord = _annotationRecordDataAccessor.Get(_currentIndex.Value - 1);
                    if (lastFrameRecord.W > 0 && lastFrameRecord.H > 0)
                    {
                        _shapeTextBoxIsModifiedByUser = false;
                        viewModel.X = lastFrameRecord.X;
                        viewModel.Y = lastFrameRecord.Y;
                        viewModel.W = lastFrameRecord.W;
                        viewModel.H = lastFrameRecord.H;
                        _shapeTextBoxIsModifiedByUser = true;
                    }
                }
            }
        }

        private void SuperResolutionInitializeWithCurrentFrame()
        {
            _drawCurrentTargetNativeHelper.initialize(DecodeImage(_currentImage), Convert.ToUInt64(_currentImage.PixelWidth), Convert.ToUInt64(_currentImage.PixelHeight));
        }
        private void OpenFrame(int index)
        {
            if (_currentIndex.HasValue && _currentIndex.Value == index)
            {
                return;
            }

            if (_currentIndex.HasValue)
            {
                UpdateRecord();
            }

            BitmapImage image;
            string imagePath = _annotationRecordDataAccessor.GetImagePath(index);
            try
            {
                image = new BitmapImage(new Uri(_annotationRecordDataAccessor.GetImagePath(index)));
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("尝试读取图片文件 {0} 时发生错误，错误信息：{1}", imagePath, e.Message), "无法加载图片",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            _lastIndex = _currentIndex;
            _lastImage = _currentImage;

            _currentImage = image;
            _currentIndex = index;

            viewModel.CurrentIndexOfFrame = index;

            AnnotationCanvas.Background = new ImageBrush(_currentImage);
            AnnotationCanvas.Width = _currentImage.PixelWidth;
            AnnotationCanvas.Height = _currentImage.PixelHeight;

            var record = _annotationRecordDataAccessor.Get(_currentIndex.Value);

            bool needFix = false;

            if (record.X + record.W > _currentImage.PixelWidth)
            {
                record.W = _currentImage.PixelWidth - record.X;
                needFix = true;
            }

            if (record.Y + record.H > _currentImage.PixelHeight)
            {
                record.H = _currentImage.PixelHeight - record.Y;
                needFix = true;
            }

            if (needFix)
                _annotationRecordDataAccessor.Update(_currentIndex.Value, record);

            if (viewModel.IsTrackerActivated)
            {
                if (index == 0)
                {
                    SetBoundingBox(record);
                }
                else
                {
                    if (record.IsLabeled)
                    {
                        if (viewModel.IsUsingTrackerOnAnnotatedRecord)
                        {
                            if (!TrackTargetBasedOnPreviousFrame())
                                SetBoundingBox(record);
                        }
                        else
                        {
                            SetBoundingBox(record);
                        }
                    }
                    else if (record.IsFullyOccluded || record.IsOutOfView)
                    {
                        SetBoundingBox(record);
                    }
                    else
                    {
                        if (!TrackTargetBasedOnPreviousFrame())
                            SetBoundingBox(record);
                    }
                }
            }
            else
            {
                SetBoundingBox(record);
            }

            viewModel.IsFullyOccluded = record.IsFullyOccluded;
            viewModel.IsOutOfView = record.IsOutOfView;

            if (viewModel.W > 0 && viewModel.H > 0)
                _selectiveBoxCanvas.Create(new Rect(viewModel.X, viewModel.Y, viewModel.W, viewModel.H));

            if (_drawCurrentTargetNativeHelper != null)
                SuperResolutionInitializeWithCurrentFrame();
            DrawCurrentTarget();

            SetMaximumValue();
            _isUserSelectFrameInformationDataGridRow = false;
            FrameInformationGrid.UpdateLayout();
            DataGridRow row = (DataGridRow)FrameInformationGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                _isUserSelectFrameInformationDataGridRow = true;
                return;
            }

            object item = FrameInformationGrid.Items[index];
            FrameInformationGrid.ScrollIntoView(item);
            row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            _isUserSelectFrameInformationDataGridRow = true;
        }

        private void UpdateRecord()
        {
            if (_currentIndex.Value == 0)
                return;

            var currentFrameInformation = viewModel.FrameInformations[_currentIndex.Value];

            currentFrameInformation.X = viewModel.X;
            currentFrameInformation.Y = viewModel.Y;
            currentFrameInformation.W = viewModel.W;
            currentFrameInformation.H = viewModel.H;
            currentFrameInformation.IsFullOccluded = viewModel.IsFullyOccluded;
            currentFrameInformation.IsOutOfView = viewModel.IsOutOfView;

            viewModel.FrameInformations[_currentIndex.Value] = currentFrameInformation;

            var record = _annotationRecordDataAccessor.Get(_currentIndex.Value);
            record.X = viewModel.X;
            record.Y = viewModel.Y;
            record.W = viewModel.W;
            record.H = viewModel.H;
            record.IsFullyOccluded = viewModel.IsFullyOccluded;
            record.IsOutOfView = viewModel.IsOutOfView;
            _annotationRecordDataAccessor.Update(_currentIndex.Value, record);
        }

        private void SubmitRecord()
        {
            if (_currentIndex.Value == 0)
                return;

            var record = _annotationRecordDataAccessor.Get(_currentIndex.Value);

            if (record.W == 0 || record.H == 0 && !record.IsFullyOccluded && !record.IsOutOfView)
                return;

            record.IsLabeled = true;
            _annotationRecordDataAccessor.Update(_currentIndex.Value, record);

            var currentFrameInformation = viewModel.FrameInformations[_currentIndex.Value];
            currentFrameInformation.IsLabeled = true;
            viewModel.FrameInformations[_currentIndex.Value] = currentFrameInformation;

            _annotationRecordDataAccessor.Submit(_currentIndex.Value);

            if (viewModel.IsUpdateMatlabRecordOnSubmit)
                _annotationRecordDataAccessor.UpdateMatlabFile();
        }

        private void ButtonBrowseDatasetFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "res 文件|res.mat;res.txt";
            openFileDialog.Multiselect = false;
            bool? isGotResult = openFileDialog.ShowDialog();
            if (!isGotResult.HasValue || !isGotResult.Value)
                return;

            string sequencePath = openFileDialog.FileName;
            sequencePath = Directory.GetParent(sequencePath).FullName;

            Reset();
            OpenSequence(sequencePath);

            TextBoxDatasetFilePath.Text = openFileDialog.FileName;
        }

        private void ClearRecord()
        {
            _shapeTextBoxIsModifiedByUser = false;
            viewModel.X = 0;
            viewModel.Y = 0;
            viewModel.W = 0;
            viewModel.H = 0;
            _shapeTextBoxIsModifiedByUser = true;
            viewModel.IsFullyOccluded = false;
            viewModel.IsOutOfView = false;
            _selectiveBoxCanvas.Clear();
            DrawCurrentTarget();
            SetMaximumValue();
        }

        private void MainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _selectiveBoxCanvas.OnMouseUp(sender, e);
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            _selectiveBoxCanvas.OnMouseMove(sender, e);
        }

        private void UpdateAndSubmitRecord()
        {
            UpdateRecord();
            SubmitRecord();
        }

        private void UpdateAndSubmitAndGotoNextFrameRecord()
        {
            UpdateRecord();
            SubmitRecord();
            OpenNextFrame();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
                _selectiveBoxCanvas.MoveUp();
            else if (e.Key == Key.A)
                _selectiveBoxCanvas.MoveLeft();
            else if (e.Key == Key.S)
                _selectiveBoxCanvas.MoveDown();
            else if (e.Key == Key.D)
                _selectiveBoxCanvas.MoveRight();
            else if (e.Key == Key.I)
                _selectiveBoxCanvas.ExpandBottom();
            else if (e.Key == Key.J)
                _selectiveBoxCanvas.ShrinkRight();
            else if (e.Key == Key.K)
                _selectiveBoxCanvas.ShrinkBottom();
            else if (e.Key == Key.L)
                _selectiveBoxCanvas.ExpandRight();
            else if (e.Key == Key.D1)
                viewModel.IsFullyOccluded = !viewModel.IsFullyOccluded;
            else if (e.Key == Key.D2)
                viewModel.IsOutOfView = !viewModel.IsOutOfView;
            else if (e.Key == Key.Space)
                UpdateAndSubmitAndGotoNextFrameRecord();
            else if (e.Key == Key.Back)
                ClearRecord();
            else if (e.Key == Key.Down)
                OpenNextFrame();
            else if (e.Key == Key.Up)
                OpenPreviousFrame();
            else if (e.Key == Key.R)
                RetrackEvent();
            else if (e.Key == Key.C)
                CopyLastFrame();
            else if (e.Key == Key.OemPlus || e.Key == Key.Add)
                CurrentTargetContextMinus();
            else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                CurrentTargetContextPlus();
            e.Handled = true;
        }

        private void CurrentTargetContextPlus()
        {
            _currentTargetViewContextPercent += 5;
            DrawCurrentTarget();
        }

        private void CurrentTargetContextMinus()
        {
            if (_currentTargetViewContextPercent >= 5)
                _currentTargetViewContextPercent -= 5;
            DrawCurrentTarget();
        }

        private bool _shapeTextBoxIsModifiedByUser = true;

        private void SetMaximumValue()
        {
            int width = (int)Math.Floor(AnnotationCanvas.Width);
            int height = (int)Math.Floor(AnnotationCanvas.Height);
            if (width > 0 && height > 0)
            {
                viewModel.MaximumOfX = width - viewModel.W;
                viewModel.MaximumOfY = height - viewModel.H;
                viewModel.MaximumOfW = width - viewModel.X;
                viewModel.MaximumOfH = height - viewModel.Y;
            }
        }

        private void ShapeTextBox_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_shapeTextBoxIsModifiedByUser)
                return;

            _selectiveBoxCanvas.SetShape(new Rect(viewModel.X, viewModel.Y, viewModel.W, viewModel.H));

            DrawCurrentTarget();
            SetMaximumValue();
        }

        private void OpenPreviousFrame()
        {
            if (_currentIndex > 0)
                OpenFrame(_currentIndex.Value - 1);
        }

        private void OpenNextFrame()
        {
            if (_currentIndex < _annotationRecordDataAccessor.GetNumberOfRecords() - 1)
                OpenFrame(_currentIndex.Value + 1);
            else
            {
                if (viewModel.IsUpdateMatlabRecordOnAppropriateOpportunity)
                    _annotationRecordDataAccessor.UpdateMatlabFile();
                MessageBox.Show("已到达最后一帧");
            }
        }

        private void PreviousFrameButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenPreviousFrame();
        }

        private void NextFrameButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenNextFrame();
        }

        private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateAndSubmitAndGotoNextFrameRecord();
        }

        private void RetrackEvent()
        {
            if (!TrackTargetBasedOnPreviousFrame())
            {
                MessageBox.Show("无法跟踪当前目标");
                return;
            }

            DrawCurrentTarget();
            SetMaximumValue();
        }

        private void RetrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            RetrackEvent();
        }

        private void FrameJump_OnClick(object sender, RoutedEventArgs e)
        {
            int index = viewModel.FindUnlabeledFrameIndex();
            if (index == viewModel.TotalNumberOfFrames)
                MessageBox.Show("所有帧已标注");
            else
            {
                OpenFrame(index);
            }
        }

        //private void FrameInformationsRow_DoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    DataGridRow row = sender as DataGridRow;
        //    int index = row.GetIndex();
        //    OpenFrame(index);
        //}

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isUserSelectFrameInformationDataGridRow)
                return;

            var selectedItems = e.AddedItems;

            if (selectedItems.Count > 0)
            {
                var item = selectedItems[0];
                if (item is MainWindowViewModel.FrameInformation)
                {
                    var frameInformation = (MainWindowViewModel.FrameInformation)item;
                    OpenFrame(frameInformation.Id - 1);
                }
            }
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            if (_annotationRecordDataAccessor == null)
                return;
            if (viewModel.IsUpdateMatlabRecordOnAppropriateOpportunity)
                _annotationRecordDataAccessor.UpdateMatlabFile();
        }

        private void UpdateMatlabRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_annotationRecordDataAccessor == null)
                return;

            _annotationRecordDataAccessor.UpdateMatlabFile();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            if (_annotationRecordDataAccessor == null)
                return;
            if (viewModel.IsUpdateMatlabRecordOnAppropriateOpportunity)
                _annotationRecordDataAccessor.UpdateMatlabFile();
        }

        private void CopyLastFrame()
        {
            int currentIndex = _currentIndex.Value;
            if (currentIndex == 0)
                return;

            var record = _annotationRecordDataAccessor.Get(currentIndex - 1);

            _shapeTextBoxIsModifiedByUser = false;
            viewModel.X = record.X;
            viewModel.Y = record.Y;
            viewModel.W = record.W;
            viewModel.H = record.H;
            viewModel.IsFullyOccluded = record.IsFullyOccluded;
            viewModel.IsOutOfView = record.IsOutOfView;
            _shapeTextBoxIsModifiedByUser = true;
            if (record.W > 0 && record.H > 0)
            {
                _selectiveBoxCanvas.Create(new Rect(record.X, record.Y, record.W, record.H));
                DrawCurrentTarget();
                SetMaximumValue();
            }
        }

        private void CopyLastFrameButton_OnClick(object sender, RoutedEventArgs e)
        {
            CopyLastFrame();
        }

        private void DeleteRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearRecord();
        }

        private void EnableSuperResolutionAlgorithmButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _drawCurrentTargetNativeHelper = new DrawCurrentTargetNativeHelper();
            if (_currentImage != null)
            {
                SuperResolutionInitializeWithCurrentFrame();
                DrawCurrentTarget();
            }
        }

        private void EnableSuperResolutionAlgorithmButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _drawCurrentTargetNativeHelper.Dispose();
            _drawCurrentTargetNativeHelper = null;
            if (_currentImage != null)
            {
                DrawCurrentTarget();
            }
        }
    }
}
