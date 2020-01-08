using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnotationTool
{
    class ShapeRectangle
    {
        private readonly Canvas _canvas;
        private Rectangle _rectangle;
        private Ellipse _tlAnchor; // top-left 
        private Ellipse _tmAnchor; // top-middle
        private Ellipse _trAnchor; // top-right
        private Ellipse _mlAnchor; // middle-left
        private Ellipse _mrAnchor; // middle-right
        private Ellipse _blAnchor; // bottom-left
        private Ellipse _bmAnchor; // bottom-middle
        private Ellipse _brAnchor; // bottom-right

        public delegate void ShapeChangedHandler(Rect shape);

        public event ShapeChangedHandler OnShapeChanged;

        public ShapeRectangle(Canvas canvas)
        {
            _canvas = canvas;
        }

        private readonly SolidColorBrush _rectangleStrokeColor = new SolidColorBrush(Colors.White);
        private double _rectangleStrokeThickness = 2;
        private readonly SolidColorBrush _anchorColor = new SolidColorBrush(Colors.Red);
        private double _anchorSize = 2;

        public Rect GetShape()
        {
            return new Rect(Canvas.GetLeft(_rectangle), Canvas.GetTop(_rectangle), _rectangle.Width, _rectangle.Height);
        }

        public void UpdateOperatableObjectSize(double width, double height)
        {
            double size = Math.Sqrt(width * height) * 0.005;
            if (size < 1)
                size = 1;
            if (_rectangleStrokeThickness != size)
            {
                if (size / 2 < 1)
                    _rectangleStrokeThickness = 1;
                else
                    _rectangleStrokeThickness = size / 2;
                _anchorSize = size;
                
                _tlAnchor.Width = _tlAnchor.Height = _anchorSize;
                _tmAnchor.Width = _tmAnchor.Height = _anchorSize;
                _trAnchor.Width = _trAnchor.Height = _anchorSize;
                _mlAnchor.Width = _mlAnchor.Height = _anchorSize;
                _mrAnchor.Width = _mrAnchor.Height = _anchorSize;
                _blAnchor.Width = _blAnchor.Height = _anchorSize;
                _bmAnchor.Width = _bmAnchor.Height = _anchorSize;
                _brAnchor.Width = _brAnchor.Height = _anchorSize;
            }
        }

        public void FitToSize(Size size)
        {
            if (_rectangle == null)
                return;
            
            Rect shape = GetShape();
            if (shape.X + shape.Width >= size.Width)
                shape.Width = size.Width - 1;
            if (shape.Y + shape.Height >= size.Height)
                shape.Height = size.Height - 1;

            SetShape(shape);
            UpdateOperatableObjectSize(size.Width, size.Height);
        }

        public void Create(Rect shape)
        {
            _rectangle = new Rectangle();
            _rectangle.Stroke = _rectangleStrokeColor;
            _rectangle.Fill = new SolidColorBrush(Colors.Transparent);
            _tlAnchor = new Ellipse();
            _tlAnchor.Fill = _anchorColor;
            _tmAnchor = new Ellipse();
            _tmAnchor.Fill = _anchorColor;
            _trAnchor = new Ellipse();
            _trAnchor.Fill = _anchorColor;
            _mlAnchor = new Ellipse();
            _mlAnchor.Fill = _anchorColor;
            _mrAnchor = new Ellipse();
            _mrAnchor.Fill = _anchorColor;
            _blAnchor = new Ellipse();
            _blAnchor.Fill = _anchorColor;
            _bmAnchor = new Ellipse();
            _bmAnchor.Fill = _anchorColor;
            _brAnchor = new Ellipse();
            _brAnchor.Fill = _anchorColor;

            UpdateOperatableObjectSize(_canvas.Width, _canvas.Height);

            _rectangle.Cursor = Cursors.SizeAll;
            _tlAnchor.Cursor = Cursors.SizeNWSE;
            _tmAnchor.Cursor = Cursors.SizeNS;
            _trAnchor.Cursor = Cursors.SizeNESW;
            _mlAnchor.Cursor = Cursors.SizeWE;
            _mrAnchor.Cursor = Cursors.SizeWE;
            _blAnchor.Cursor = Cursors.SizeNESW;
            _bmAnchor.Cursor = Cursors.SizeNS;
            _brAnchor.Cursor = Cursors.SizeNWSE;

            _rectangle.MouseDown += RectangleOnMouseDown;
            _tlAnchor.MouseDown += TlAnchorOnMouseDown;
            _tmAnchor.MouseDown += TmAnchorOnMouseDown;
            _trAnchor.MouseDown += TrAnchorOnMouseDown;
            _mlAnchor.MouseDown += MlAnchorOnMouseDown;
            _mrAnchor.MouseDown += MrAnchorOnMouseDown;
            _blAnchor.MouseDown += BlAnchorOnMouseDown;
            _bmAnchor.MouseDown += BmAnchorOnMouseDown;
            _brAnchor.MouseDown += BrAnchorOnMouseDown;

            _canvas.Children.Add(_rectangle);
            _canvas.Children.Add(_tlAnchor);
            _canvas.Children.Add(_tmAnchor);
            _canvas.Children.Add(_trAnchor);
            _canvas.Children.Add(_mlAnchor);
            _canvas.Children.Add(_mrAnchor);
            _canvas.Children.Add(_blAnchor);
            _canvas.Children.Add(_bmAnchor);
            _canvas.Children.Add(_brAnchor);

            SetShape(shape);
        }

        public bool IsExists()
        {
            return _rectangle != null;
        }

        public void Destroy()
        {
            if (_rectangle != null)
            {
                _canvas.Children.Remove(_rectangle);
                _rectangle = null;
            }
            if (_tlAnchor != null)
            {
                _canvas.Children.Remove(_tlAnchor);
                _tlAnchor = null;
            }
            if (_tmAnchor != null)
            {
                _canvas.Children.Remove(_tmAnchor);
                _tmAnchor = null;
            }
            if (_trAnchor != null)
            {
                _canvas.Children.Remove(_trAnchor);
                _trAnchor = null;
            }
            if (_mlAnchor != null)
            {
                _canvas.Children.Remove(_mlAnchor);
                _mlAnchor = null;
            }
            if (_mrAnchor != null)
            {
                _canvas.Children.Remove(_mrAnchor);
                _mrAnchor = null;
            }
            if (_blAnchor != null)
            {
                _canvas.Children.Remove(_blAnchor);
                _blAnchor = null;
            }
            if (_bmAnchor != null)
            {
                _canvas.Children.Remove(_bmAnchor);
                _bmAnchor = null;
            }
            if (_brAnchor != null)
            {
                _canvas.Children.Remove(_brAnchor);
                _brAnchor = null;
            }
        }

        private void BrAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.BRAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void BmAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.BMAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void BlAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.BLAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void MrAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.MRAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void MlAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.MLAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void TrAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.TRAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }
        
        enum ActiveObject
        {
            Rectangle, TLAnchor, TMAnchor, TRAnchor, MLAnchor, MRAnchor, BLAnchor, BMAnchor, BRAnchor
        }

        private bool _inMovingState = false;
        private Rect _initialStateRect;
        private Point _initialPosition;
        private ActiveObject _currentActiveObject;

        private void EnterModificationMode(ActiveObject activeObject, Point currentPosition)
        {
            _initialPosition = currentPosition;
            double rectangleX = Canvas.GetLeft(_rectangle);
            double rectangleY = Canvas.GetTop(_rectangle);
            _initialStateRect = new Rect(rectangleX, rectangleY, _rectangle.Width, _rectangle.Height);
            _currentActiveObject = activeObject;
            _inMovingState = true;
        }
        
        public void ShrinkTop()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Height > 1)
            {
                shape.Y = shape.Y + 1;
                shape.Height = shape.Height - 1;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ShrinkBottom()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Height > 1)
            {
                shape.Height = shape.Height - 1;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ExpandTop()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Y >= 1)
            {
                shape.Y = shape.Y - 1;
                shape.Height = shape.Height + 1;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ExpandBottom()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            
            if (shape.Height + shape.Y + 1 <= _canvas.Height)
            {
                shape.Height = shape.Height + 1;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ExpandLeft()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.X >= 1)
            {
                shape.X--;
                shape.Width++;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ExpandRight()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.X + shape.Width + 1 <= _canvas.Width)
            {
                shape.Width++;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ShrinkLeft()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Width > 1)
            {
                shape.X++;
                shape.Width--;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void ShrinkRight()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Width > 1)
            {
                shape.Width--;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void MoveLeft()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.X >= 1)
            {
                shape.X--;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void MoveRight()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.X + shape.Width + 1 <= _canvas.Width)
            {
                shape.X++;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void MoveUp()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Y >= 1)
            {
                shape.Y--;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        public void MoveDown()
        {
            if (_rectangle == null)
                return;
            var shape = GetShape();
            if (shape.Y + shape.Height + 1 <= _canvas.Height)
            {
                shape.Y++;
                SetShape(shape);
                OnShapeChanged?.Invoke(shape);
            }
        }

        private bool IsInValidRegion(Rect currentPosition)
        {
            return IsInValidRegion(currentPosition.X, currentPosition.Y, currentPosition.Width, currentPosition.Height);
        }
        
        private bool IsInValidRegion(double X, double Y, double W, double H)
        {
            if (W <= 0 || H <= 0)
                return false;
            if (X < 0 || X + W > _canvas.Width || Y < 0 || Y + H > _canvas.Height)
                return false;
            return true;
        }

        private bool IsInValidRegion(Point currentPosition)
        {
            return IsInValidRegion(currentPosition.X, currentPosition.Y);
        }

        private bool IsInValidRegion(double X, double Y)
        {
            if (X < 0 || X >= _canvas.Width || Y < 0 || Y > _canvas.Height)
                return false;
            return true;
        }

        private bool OnMouseMove(Point currentPosition)
        {
            if (!IsInValidRegion(currentPosition))
                return false;
            
            if (_inMovingState)
            {
                if (_currentActiveObject == ActiveObject.Rectangle)
                {
                    double X = currentPosition.X - _initialPosition.X + _initialStateRect.X;
                    double Y = currentPosition.Y - _initialPosition.Y + _initialStateRect.Y;
                    double W = _initialStateRect.Width;
                    double H = _initialStateRect.Height;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetPosition(new Point(X, Y));
                }
                else if (_currentActiveObject == ActiveObject.TLAnchor)
                {
                    double X = currentPosition.X;
                    double Y = currentPosition.Y;
                    double W = _initialStateRect.Width - currentPosition.X + _initialPosition.X;
                    double H = _initialStateRect.Height - currentPosition.Y + _initialPosition.Y;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.TMAnchor)
                {
                    double X = _initialStateRect.X;
                    double Y = currentPosition.Y;
                    double W = _initialStateRect.Width;
                    double H = _initialStateRect.Height - currentPosition.Y + _initialPosition.Y;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.TRAnchor)
                {
                    double X = _initialStateRect.X;
                    double Y = currentPosition.Y;
                    double W = _initialStateRect.Width + currentPosition.X - _initialPosition.X;
                    double H = _initialStateRect.Height - currentPosition.Y + _initialPosition.Y;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.MLAnchor)
                {
                    double X = currentPosition.X;
                    double Y = _initialStateRect.Y;
                    double W = _initialStateRect.Width - currentPosition.X + _initialPosition.X;
                    double H = _initialStateRect.Height;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.MRAnchor)
                {
                    double X = _initialStateRect.X;
                    double Y = _initialStateRect.Y;
                    double W = _initialStateRect.Width + currentPosition.X - _initialPosition.X;
                    double H = _initialStateRect.Height;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.BLAnchor)
                {
                    double X = currentPosition.X;
                    double Y = _initialStateRect.Y;
                    double W = _initialStateRect.Width - currentPosition.X + _initialPosition.X;
                    double H = _initialStateRect.Height + currentPosition.Y - _initialPosition.Y;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.BMAnchor)
                {
                    double X = _initialStateRect.X;
                    double Y = _initialStateRect.Y;
                    double W = _initialStateRect.Width;
                    double H = _initialStateRect.Height + currentPosition.Y - _initialPosition.Y;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
                else if (_currentActiveObject == ActiveObject.BRAnchor)
                {
                    double X = _initialStateRect.X;
                    double Y = _initialStateRect.Y;
                    double W = _initialStateRect.Width + currentPosition.X - _initialPosition.X;
                    double H = _initialStateRect.Height + currentPosition.Y - _initialPosition.Y;
                    if (!IsInValidRegion(X, Y, W, H))
                        return true;
                    SetShape(new Rect(X, Y, W, H));
                }
            }

            return true;
        }

        public void SetPosition(Point position)
        {
            double X = position.X;
            double Y = position.Y;
            double W = _rectangle.Width;
            double H = _rectangle.Height;

            Canvas.SetLeft(_rectangle, X);
            Canvas.SetTop(_rectangle, Y);

            Canvas.SetLeft(_tlAnchor, X - _tlAnchor.Width / 2);
            Canvas.SetTop(_tlAnchor, Y - _tlAnchor.Height / 2);

            Canvas.SetLeft(_tmAnchor, X + W / 2 - _tmAnchor.Width / 2);
            Canvas.SetTop(_tmAnchor, Y - _tmAnchor.Height / 2);

            Canvas.SetLeft(_trAnchor, X + W - _trAnchor.Width / 2);
            Canvas.SetTop(_trAnchor, Y - _trAnchor.Height / 2);

            Canvas.SetLeft(_mlAnchor, X - _mlAnchor.Width / 2);
            Canvas.SetTop(_mlAnchor, Y + H / 2 - _mlAnchor.Height / 2);

            Canvas.SetLeft(_mrAnchor, X + W - _mrAnchor.Width / 2);
            Canvas.SetTop(_mrAnchor, Y + H / 2 - _mrAnchor.Height / 2);

            Canvas.SetLeft(_blAnchor, X - _blAnchor.Width / 2);
            Canvas.SetTop(_blAnchor, Y + H - _blAnchor.Height / 2);

            Canvas.SetLeft(_bmAnchor, X + W / 2 - _bmAnchor.Width / 2);
            Canvas.SetTop(_bmAnchor, Y + H - _bmAnchor.Height / 2);

            Canvas.SetLeft(_brAnchor, X + W - _brAnchor.Width / 2);
            Canvas.SetTop(_brAnchor, Y + H - _brAnchor.Height / 2);
        }

        private void LeaveModificationMode()
        {
            _inMovingState = false;
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(_canvas);

            position.X = Math.Round(position.X);
            position.Y = Math.Round(position.Y);

            if (OnMouseMove(position))
                e.Handled = true;
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(_canvas);
            if (!IsInValidRegion(position))
                return;

            if (!_inMovingState)
                return;

            position.X = Math.Round(position.X);
            position.Y = Math.Round(position.Y);

            OnMouseMove(position);
            LeaveModificationMode();
            e.Handled = true;
            OnShapeChanged?.Invoke(GetShape());
        }

        private void RectangleOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.Rectangle, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void TlAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.TLAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }

        private void TmAnchorOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnterModificationMode(ActiveObject.TMAnchor, e.GetPosition(_canvas));
            e.Handled = true;
        }
        
        public void Move(Point offset)
        {
            double X = Canvas.GetLeft(_rectangle);
            double Y = Canvas.GetTop(_rectangle);
            double W = _rectangle.Width;
            double H = _rectangle.Height;

            Canvas.SetLeft(_rectangle, X + offset.X);
            Canvas.SetTop(_rectangle, Y + offset.Y);

            Canvas.SetLeft(_tlAnchor, X + offset.X);
            Canvas.SetTop(_tlAnchor, Y + offset.Y);

            Canvas.SetLeft(_tmAnchor, X + W / 2 + offset.X);
            Canvas.SetTop(_tmAnchor, Y + offset.Y);

            Canvas.SetLeft(_trAnchor, X + W + offset.X);
            Canvas.SetTop(_trAnchor, Y + offset.Y);

            Canvas.SetLeft(_mlAnchor, X + offset.X);
            Canvas.SetTop(_mlAnchor, Y + H / 2 + offset.Y);

            Canvas.SetLeft(_mrAnchor, X + W + offset.X);
            Canvas.SetTop(_mrAnchor, Y + H / 2 + offset.Y);

            Canvas.SetLeft(_blAnchor, X + offset.X);
            Canvas.SetTop(_blAnchor, Y + H + offset.Y);

            Canvas.SetLeft(_bmAnchor, X + W / 2 + offset.X);
            Canvas.SetTop(_bmAnchor, Y + H + offset.Y);

            Canvas.SetLeft(_bmAnchor, X + W);
            Canvas.SetTop(_bmAnchor, Y + H + offset.Y);
        }

        public void SetShape(Rect shape)
        {
            Canvas.SetLeft(_rectangle, shape.X);
            Canvas.SetTop(_rectangle, shape.Y);
            _rectangle.Width = shape.Width;
            _rectangle.Height = shape.Height;

            Canvas.SetLeft(_tlAnchor, shape.X - _tlAnchor.Width / 2);
            Canvas.SetTop(_tlAnchor, shape.Y - _tlAnchor.Height / 2);

            Canvas.SetLeft(_tmAnchor, shape.X + shape.Width / 2 - _tmAnchor.Width / 2);
            Canvas.SetTop(_tmAnchor, shape.Y - _tmAnchor.Height / 2);

            Canvas.SetLeft(_trAnchor, shape.X + shape.Width - _trAnchor.Width / 2);
            Canvas.SetTop(_trAnchor, shape.Y - _trAnchor.Height / 2);

            Canvas.SetLeft(_mlAnchor, shape.X - _mlAnchor.Width / 2);
            Canvas.SetTop(_mlAnchor, shape.Y + shape.Height / 2 - _mlAnchor.Height / 2);

            Canvas.SetLeft(_mrAnchor, shape.X + shape.Width - _mrAnchor.Width / 2);
            Canvas.SetTop(_mrAnchor, shape.Y + shape.Height / 2 - _mrAnchor.Height / 2);

            Canvas.SetLeft(_blAnchor, shape.X - _blAnchor.Width / 2);
            Canvas.SetTop(_blAnchor, shape.Y + shape.Height - _blAnchor.Height / 2);

            Canvas.SetLeft(_bmAnchor, shape.X + shape.Width / 2 - _bmAnchor.Width / 2);
            Canvas.SetTop(_bmAnchor, shape.Y + shape.Height - _bmAnchor.Height / 2);

            Canvas.SetLeft(_brAnchor, shape.X + shape.Width - _brAnchor.Width / 2);
            Canvas.SetTop(_brAnchor, shape.Y + shape.Height - _brAnchor.Height / 2);
        }
    }
}
